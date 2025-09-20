using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using _Project.Scripts.UI.PlayingObjects.Column;
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
        [SerializeField] private List<ColumnController> _allColumns;

        private ColumnManager _columnManager;
        private PlayerBlockMover _playerBlockMover;
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
            _playerBlockMover = new PlayerBlockMover();
        }

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            _columnManager.SeparateColumns();
            _columnManager.SubscribeToActiveCells(OnBeginDrag, OnEndDrag);
            CenterActiveColumns();
            _columnManager.ResolveGameZone(_cts.Token).Forget();
        }

        private void CenterActiveColumns()
        {
            var activeColumns = _allColumns.Where(c => c.Model.ColumnIsActive).ToList();
            if (activeColumns.Count == 0) return;

            var leftEdgeFromAll = _allColumns.Min(c => c.RectTransform.anchoredPosition.x - c.RectTransform.rect.width / 2f);
            var rightEdgeFromAll = _allColumns.Max(c => c.RectTransform.anchoredPosition.x + c.RectTransform.rect.width / 2f);
            var leftEdgeFromActive = activeColumns.Min(c => c.RectTransform.anchoredPosition.x - c.RectTransform.rect.width / 2f);
            var rightEdgeFromActive = activeColumns.Max(c => c.RectTransform.anchoredPosition.x + c.RectTransform.rect.width / 2f);
            var centerX = ((leftEdgeFromActive - leftEdgeFromAll) + (rightEdgeFromActive - rightEdgeFromAll)) / 2f;

            _rectTransform.anchoredPosition = new Vector2(-centerX, _rectTransform.anchoredPosition.y);
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
    
            if (!oldCell.PlayableBlockPresenter.IsInteractable) return;
    
            if (targetCell.PlayableBlockPresenter != null && !targetCell.PlayableBlockPresenter.IsInteractable)
                return;

            if (direction == Vector2Int.up && targetCell.PlayableBlockPresenter == null) return;

            if (targetCell.PlayableBlockPresenter == null)
            {
                await _playerBlockMover.MoveToEmptyCell(oldCell, targetCell, direction, _cts.Token);
                await _columnManager.NormalizeGameZone(_cts.Token);
            }
            else
            {
                await _playerBlockMover.SwapBlocks(oldCell, targetCell, direction, _cts.Token);
            }

            await _columnManager.ResolveGameZone(_cts.Token);
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
