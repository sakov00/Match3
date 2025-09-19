using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        public void CheckGameZone()
        {
            var width = ActiveColumns.Count;
            var height = ActiveColumns[0].Cells.Count;

            var grid = BuildGrid(width, height);
            var visited = new bool[width, height];
            var lineMarks = new bool[width, height];

            MarkHorizontalLines(grid, lineMarks, width, height);
            MarkVerticalLines(grid, lineMarks, width, height);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (!lineMarks[x, y] || visited[x, y]) continue;
                    
                    var connected = new List<(int x, int y)>();
                    var groupId = grid[x, y];
                    CollectConnected(x, y, groupId, grid, visited, connected);

                    foreach (var pos in connected)
                    {
                        ActiveColumns[pos.x].Cells[pos.y].PlayableBlockPresenter.View.Hide();
                        ActiveColumns[pos.x].Cells[pos.y].PlayableBlockPresenter = null;
                    }
                }
            }
        }

        private void CollectConnected(int x, int y, int groupId, int[,] grid, bool[,] visited, List<(int x, int y)> connected)
        {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((x, y));

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                if (cx < 0 || cx >= width || cy < 0 || cy >= height)
                    continue;
                if (visited[cx, cy] || grid[cx, cy] != groupId)
                    continue;

                visited[cx, cy] = true;
                connected.Add((cx, cy));

                queue.Enqueue((cx + 1, cy));
                queue.Enqueue((cx - 1, cy));
                queue.Enqueue((cx, cy + 1));
                queue.Enqueue((cx, cy - 1));
            }
        }

        private int[,] BuildGrid(int width, int height)
        {
            var grid = new int[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    grid[x, y] = ActiveColumns[x].Cells[y].PlayableBlockPresenter?.Model.GroupId ?? -1;
                }
            }
            return grid;
        }

        private void MarkHorizontalLines(int[,] grid, bool[,] lineMarks, int width, int height)
        {
            for (var y = 0; y < height; y++)
            {
                var count = 1;
                for (var x = 1; x < width; x++)
                {
                    if (grid[x, y] == grid[x - 1, y] && grid[x, y] != -1)
                        count++;
                    else
                        count = 1;

                    if (count < 3) continue;
                    for (var k = 0; k < count; k++)
                        lineMarks[x - k, y] = true;
                }
            }
        }

        private void MarkVerticalLines(int[,] grid, bool[,] lineMarks, int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                var count = 1;
                for (var y = 1; y < height; y++)
                {
                    if (grid[x, y] == grid[x, y - 1] && grid[x, y] != -1)
                        count++;
                    else
                        count = 1;

                    if (count < 3) continue;
                    for (int k = 0; k < count; k++)
                        lineMarks[x, y - k] = true;
                }
            }
        }
        
        public void NormalizeGameZone()
        {
            // DropBlockDown()
        }
        
        public async UniTask DropBlockDown(CellController cell, Column column, CancellationToken token)
        {
            var block = cell.PlayableBlockPresenter;
            var rowIndex = cell.Model.RowIndex;
            
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

            await block.transform.DOMove(lowestEmptyCell.transform.position, 0.25f)
                .Play().WithCancellation(token);
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

