using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public class GameZone : MonoBehaviour
    {
        [Inject] private ObjectsRegistry _objectsRegistry;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private List<Column> _allColumns;

        private ColumnManager _columnManager;
        private BlockMover _blockMover;
        private Vector2 _dragStartPos;
        private CancellationTokenSource _cts;

        private void OnValidate()
        {
            _rectTransform ??= GetComponent<RectTransform>();
        }

        private void Awake()
        {
            InjectManager.Inject(this);
            _columnManager = new ColumnManager(_allColumns);
            _blockMover = new BlockMover();
        }

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            _columnManager.SeparateColumns();
            _columnManager.SubscribeToActiveCells(OnBeginDrag, OnEndDrag);
            CenterActiveColumns();
        }

        private void CenterActiveColumns()
        {
            var offset = _columnManager.InactiveColumns.Sum(c => c.FieldSize.x) / 2;
            _rectTransform.anchoredPosition = new Vector2(offset, _rectTransform.anchoredPosition.y);
        }

        private void OnBeginDrag(PointerEventData eventData, CellController oldCell)
        {
            _dragStartPos = eventData.position;
        }

        private async void OnEndDrag(PointerEventData eventData, CellController oldCell)
        {
            var delta = eventData.position - _dragStartPos;
            if (delta.magnitude < 20f) return;

            var direction = GetDirection(delta);
            var targetCell = GetTargetCell(oldCell, direction);
            
            if (oldCell.PlayableBlockPresenter == null || targetCell == null) return;
            if (direction == Vector2Int.up && targetCell.PlayableBlockPresenter == null) return;

            if (targetCell.PlayableBlockPresenter == null)
            {
                await _blockMover.MoveToEmptyCell(oldCell, targetCell, direction, _cts.Token);
                if (direction == Vector2Int.left || direction == Vector2Int.right)
                {
                    await _blockMover.DropBlockDown(targetCell, _columnManager.ActiveColumns[targetCell.Model.ColumnIndex], _cts.Token);
                }
            }
            else
            {
                await _blockMover.SwapBlocks(oldCell, targetCell, direction, _cts.Token);
            }
        }
        
        private Vector2Int GetDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        private CellController GetTargetCell(CellController oldCell, Vector2Int direction)
        {
            int newColumnIndex = oldCell.Model.ColumnIndex + direction.x;
            int newRowIndex = oldCell.Model.RowIndex + direction.y;

            if (newColumnIndex < 0 || newColumnIndex >= _columnManager.ActiveColumns.Count)
                return null;

            var column = _columnManager.ActiveColumns[newColumnIndex];
            if (newRowIndex < 0 || newRowIndex >= column.Cells.Count)
                return null;

            return column.Cells[newRowIndex];
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _columnManager.UnsubscribeFromAllCells(OnBeginDrag, OnEndDrag);
        }
    }
}
