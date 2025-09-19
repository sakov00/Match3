using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.UI.PlayingObjects.Cell;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public class ColumnManager
    {
        public List<Column> AllColumns { get; private set; }
        public List<Column> ActiveColumns { get; private set; } = new();
        public List<Column> InactiveColumns { get; private set; } = new();

        public ColumnManager(List<Column> allColumns)
        {
            AllColumns = allColumns;
        }

        public void SeparateColumns()
        {
            ActiveColumns.Clear();
            InactiveColumns.Clear();

            var activeIndices = AllColumns
                .Select((col, index) => new { col, index })
                .Where(x => x.col.Cells.Any(c => c.PlayableBlockPresenter != null))
                .Select(x => x.index)
                .ToList();

            if (!activeIndices.Any())
            {
                InactiveColumns.AddRange(AllColumns);
                return;
            }

            var firstActive = activeIndices.First();
            var lastActive = activeIndices.Last();

            ActiveColumns.AddRange(AllColumns.Skip(firstActive).Take(lastActive - firstActive + 1));
            InactiveColumns.AddRange(AllColumns.Take(firstActive));
            InactiveColumns.AddRange(AllColumns.Skip(lastActive + 1));
        }

        public void SubscribeToActiveCells(System.Action<PointerEventData, CellController> onBeginDrag,
            System.Action<PointerEventData, CellController> onEndDrag)
        {
            foreach (var column in ActiveColumns)
            {
                foreach (var cell in column.Cells)
                {
                    cell.OnBeginedDrag += onBeginDrag;
                    cell.OnEndedDrag += onEndDrag;
                }
            }
        }

        public void UnsubscribeFromAllCells(System.Action<PointerEventData, CellController> onBeginDrag,
            System.Action<PointerEventData, CellController> onEndDrag)
        {
            foreach (var column in AllColumns)
            {
                foreach (var cell in column.Cells)
                {
                    cell.OnBeginedDrag -= onBeginDrag;
                    cell.OnEndedDrag -= onEndDrag;
                }
            }
        }
    }
}

