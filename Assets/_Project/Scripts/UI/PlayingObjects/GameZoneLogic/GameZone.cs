using System.Collections.Generic;
using System.Linq;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects
{
    public class GameZone : MonoBehaviour
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private List<Column> _allColumns;
        
        private readonly List<Column> _activeColumns = new();
        private readonly List<Column> _inactiveColumns = new();
        private Vector2 _dragStartPos;

        private void OnValidate()
        {
            _rectTransform ??= GetComponent<RectTransform>();
        }

        private void Awake()
        {
            InjectManager.Inject(this);
        }

        public void Initialize()
        {
            SeparateColumns();
            SubscribeToActiveCells();
            CenterActiveColumns();
        }

        private void SeparateColumns()
        {
            _activeColumns.Clear();
            _inactiveColumns.Clear();

            var activeIndices = _allColumns
                .Select((col, index) => new { col, index })
                .Where(x => x.col.Cells.Any(c => c.PlayableBlockPresenter != null))
                .Select(x => x.index)
                .ToList();

            if (!activeIndices.Any())
            {
                _inactiveColumns.AddRange(_allColumns);
                return;
            }

            var firstActive = activeIndices.First();
            var lastActive = activeIndices.Last();

            _activeColumns.AddRange(_allColumns.Skip(firstActive).Take(lastActive - firstActive + 1));
            _inactiveColumns.AddRange(_allColumns.Take(firstActive));
            _inactiveColumns.AddRange(_allColumns.Skip(lastActive + 1));
        }

        private void SubscribeToActiveCells()
        {
            foreach (var column in _activeColumns)
            {
                foreach (var cell in column.Cells)
                {
                    cell.OnBeginedDrag += OnBeginDrag;
                    cell.OnEndedDrag += OnEndedDrag;
                }
            }
        }

        private void CenterActiveColumns()
        {
            _rectTransform.anchoredPosition = new Vector2(0, _rectTransform.anchoredPosition.y);
            _rectTransform.anchoredPosition += new Vector2(_inactiveColumns.Sum(x => x.FieldSize.x) / 2, 0);
        }

        private void OnBeginDrag(PointerEventData eventData, CellController oldCell)
        {
            _dragStartPos = eventData.position;
        }

        private void OnEndedDrag(PointerEventData eventData, CellController oldCell)
        {
            var dragEndPos = eventData.position;
            var delta = dragEndPos - _dragStartPos;

            if (delta.magnitude < 20f)
                return;

            var direction = GetDirection(delta);
            var targetCell = GetTargetCell(oldCell, direction);
            
            if ((direction == Vector2Int.up && targetCell.PlayableBlockPresenter == null) 
                || oldCell.PlayableBlockPresenter == null || targetCell == null)
                return;

            if (targetCell.PlayableBlockPresenter == null)
            {
                MoveToEmptyCell(oldCell, targetCell, direction).Forget();
            }
            else
            {
                SwapBlocks(oldCell, targetCell, direction).Forget();
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
            var newColumnIndex = oldCell.Model.ColumnIndex + direction.x;
            var newRowIndex = oldCell.Model.RowIndex + direction.y;

            if (newColumnIndex < 0 || newColumnIndex >= _activeColumns.Count)
                return null;

            var column = _activeColumns[newColumnIndex];
            if (newRowIndex < 0 || newRowIndex >= column.Cells.Count)
                return null;

            return column.Cells[newRowIndex];
        }

        private async UniTaskVoid MoveToEmptyCell(CellController oldCell, CellController targetCell, Vector2Int direction)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;
            var duration = 0.25f;

            if (direction == Vector2Int.up)
                oldBlock.transform.SetParent(targetCell.transform, true);
            if (direction == Vector2Int.right || direction == Vector2Int.left)
                oldBlock.transform.SetParent(targetCell.transform, true);
            
            await oldBlock.transform.DOMove(targetCell.transform.position, duration).Play();
            
            if (direction == Vector2Int.down)
                oldBlock.transform.SetParent(targetCell.transform, true);

            oldCell.PlayableBlockPresenter = null;
            targetCell.PlayableBlockPresenter = oldBlock;
            
            if (direction == Vector2Int.left || direction == Vector2Int.right)
            {
                await DropBlockDown(targetCell);
            }
        }
        
        private async UniTask DropBlockDown(CellController cell)
        {
            var block = cell.PlayableBlockPresenter;
            var columnIndex = cell.Model.ColumnIndex;
            var rowIndex = cell.Model.RowIndex;
            var column = _activeColumns[columnIndex];
            
            if(rowIndex - 1 < 0) return;

            CellController lowestEmptyCell = null;
            for (int row = rowIndex - 1; row >= 0 ; row--)
            {
                if (column.Cells[row].PlayableBlockPresenter == null)
                {
                    lowestEmptyCell = column.Cells[row];
                }
                else
                {
                    break;
                }
            }

            if (lowestEmptyCell == null) return;

            cell.PlayableBlockPresenter = null;
            lowestEmptyCell.PlayableBlockPresenter = block;
            block.transform.SetParent(lowestEmptyCell.transform, true);

            await block.transform.DOMove(lowestEmptyCell.transform.position, 0.25f).Play();
        }

        private async UniTaskVoid SwapBlocks(CellController oldCell, CellController targetCell, Vector2Int direction)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;
            var targetBlock = targetCell.PlayableBlockPresenter;

            if (direction == Vector2Int.up)
                oldBlock.transform.SetParent(targetCell.transform, true);
            if (direction == Vector2Int.down)
                targetBlock.transform.SetParent(oldCell.transform, true);
            if (direction == Vector2Int.right || direction == Vector2Int.left)
            {
                oldBlock.transform.SetParent(targetCell.transform, true);
                targetBlock.transform.SetParent(oldCell.transform, true);
            }

            var sequence = DOTween.Sequence();
            sequence.Append(oldBlock.transform.DOMove(targetCell.transform.position, 0.25f));
            sequence.Join(targetBlock.transform.DOMove(oldCell.transform.position, 0.25f));
            await sequence.Play();
            
            if (direction == Vector2Int.up)
                targetBlock.transform.SetParent(oldCell.transform, true);
            if (direction == Vector2Int.down)
                oldBlock.transform.SetParent(targetCell.transform, true);

            oldCell.PlayableBlockPresenter = targetBlock;
            targetCell.PlayableBlockPresenter = oldBlock;
        }
        
        public void Dispose()
        {
            _allColumns.ForEach(x => x.Cells.ForEach(y=>
            {
                y.OnBeginedDrag -= OnBeginDrag;
                y.OnEndedDrag -= OnEndedDrag;
            }));
        }
    }
}