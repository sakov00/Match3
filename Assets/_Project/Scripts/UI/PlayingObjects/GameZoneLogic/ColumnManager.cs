using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts.Enums;
using _Project.Scripts.UI.PlayingObjects.Cell;
using _Project.Scripts.UI.PlayingObjects.Column;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public class ColumnManager
    {
        public List<ColumnController> AllColumns { get; private set; }
        public List<ColumnController> ActiveColumns { get; private set; } = new();
        public List<ColumnController> InactiveColumns { get; private set; } = new();

        public ColumnManager(List<ColumnController> allColumns)
        {
            AllColumns = allColumns;
        }

        public void SeparateColumns()
        {
            ActiveColumns.Clear();
            InactiveColumns.Clear();
            ActiveColumns = AllColumns.Where(x => x.Model.ColumnIsActive == true).ToList();
            InactiveColumns = AllColumns.Where(x => x.Model.ColumnIsActive == false).ToList();
        }
        
        public async UniTask ResolveGameZone(CancellationToken token)
        {
            try
            {
                bool changed;
                do
                {
                    token.ThrowIfCancellationRequested();

                    changed = false;
                    if (await CheckGameZone(token))
                        changed = true;

                    token.ThrowIfCancellationRequested();

                    if (await NormalizeGameZone(token))
                        changed = true;

                } while (changed);
            }
            catch (OperationCanceledException) { }
        }

        private async UniTask<bool> CheckGameZone(CancellationToken token)
        {
            var width = ActiveColumns.Count;
            var height = ActiveColumns[0].Cells.Count;

            var grid = BuildGrid(width, height);
            var visited = new bool[width, height];
            var lineMarks = new bool[width, height];

            MarkHorizontalLines(grid, lineMarks, width, height);
            MarkVerticalLines(grid, lineMarks, width, height);

            var removedAny = false;
            var tasks = new List<UniTask>();
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    token.ThrowIfCancellationRequested();
                    
                    if (!lineMarks[x, y] || visited[x, y]) continue;

                    var connected = new List<(int x, int y)>();
                    var groupId = grid[x, y];
                    CollectConnected(x, y, groupId, grid, visited, connected);

                    foreach (var pos in connected)
                    {
                        var cell = ActiveColumns[pos.x].Cells[pos.y];
                        var task = cell.PlayableBlockPresenter.DestroyAnimStart(token)
                            .ContinueWith(() => cell.PlayableBlockPresenter = null);
                        tasks.Add(task);
                    }

                    removedAny = true;
                }
            }
            token.ThrowIfCancellationRequested();
            await UniTask.WhenAll(tasks).AttachExternalCancellation(token);
            return removedAny;
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
        
        public async UniTask<bool> NormalizeGameZone(CancellationToken token)
        {
            var moved = false;
            var tasks = new List<UniTask<bool>>();

            foreach (var column in ActiveColumns)
            {
                for (var row = column.Cells.Count - 1; row >= 0; row--)
                {
                    var cell = column.Cells[row];
                    if (cell.PlayableBlockPresenter != null)
                    {
                        tasks.Add(DropBlockDown(cell, column, token));
                    }
                }
            }

            if (tasks.Count <= 0) return moved;
            
            var results = await UniTask.WhenAll(tasks);
            moved = results.Any(r => r);

            return moved;
        }
        
        private async UniTask<bool> DropBlockDown(CellController cell, ColumnController columnController, CancellationToken token)
        {
            var block = cell.PlayableBlockPresenter;
            try
            {
                var rowIndex = cell.Model.RowIndex;
                if (rowIndex - 1 < 0) return false;

                CellController lowestEmptyCell = null;
                for (var row = rowIndex - 1; row >= 0; row--)
                {
                    if (columnController.Cells[row].PlayableBlockPresenter == null)
                        lowestEmptyCell = columnController.Cells[row];
                    else
                        break;
                }

                if (lowestEmptyCell == null) return false;

                block.transform.DOComplete(true);
                token.Register(() => block.transform.DOComplete(true));

                cell.PlayableBlockPresenter = null;
                lowestEmptyCell.PlayableBlockPresenter = block;
                block.transform.SetParent(lowestEmptyCell.transform, true);

                token.ThrowIfCancellationRequested();
                block.Model.State = BlockState.Falling;
                await block.transform.DOMove(lowestEmptyCell.transform.position, 0.25f);
                block.Model.State = BlockState.Idle;
                return true;
            }
            catch (OperationCanceledException)
            {
                block.transform.position = Vector2.zero;
                block.transform.localScale = Vector3.zero;
                return false;
            }
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

