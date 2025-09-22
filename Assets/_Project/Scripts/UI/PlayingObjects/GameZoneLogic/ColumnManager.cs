using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts.Enums;
using _Project.Scripts.UI.PlayingObjects.Cell;
using _Project.Scripts.UI.PlayingObjects.Column;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
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
                    changed = false;
                    
                    token.ThrowIfCancellationRequested();
                    if (await NormalizeGameZone(token)) changed = true;
                    
                    token.ThrowIfCancellationRequested();
                    if (await CheckGameZone(token)) changed = true;
                } while (changed);
            }
            catch (OperationCanceledException) { }
        }

        public void MarkFullPrediction()
        {
            if (ActiveColumns.Count == 0) return;

            var width = ActiveColumns.Count;
            var height = ActiveColumns[0].Cells.Count;

            var grid = BuildGrid(width, height);

            bool changed;

            do
            {
                changed = false;
                for (int x = 0; x < width; x++)
                {
                    int emptyCount = 0;

                    for (int y = 0; y < height; y++)
                    {
                        var block = grid[x, y];

                        if (block == null || block.Model.State == BlockState.Destroying || block.Model.State == BlockState.PredictDestroy)
                        {
                            emptyCount++;
                            continue;
                        }

                        if (emptyCount > 0)
                        {
                            grid[x, y - emptyCount] = block;
                            grid[x, y] = null;
                            block.Model.State = BlockState.PredictFalling;
                            changed = true;
                        }
                    }
                }

                var lineMarks = new bool[width, height];
                MarkHorizontalLines(grid, lineMarks, width, height);
                MarkVerticalLines(grid, lineMarks, width, height);

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var block = grid[x, y];
                        if (block == null || !lineMarks[x, y] || block.Model.State == BlockState.Destroying ||
                            block.Model.State == BlockState.PredictDestroy) continue;

                        CollectConnected(x, y, block, grid, new bool[width, height], out var connected);

                        foreach (var pos in connected)
                        {
                            var b = grid[pos.x, pos.y];
                            if (b != null && block.Model.State != BlockState.Destroying && b.Model.State != BlockState.PredictDestroy)
                            {
                                b.Model.State = BlockState.PredictDestroy;
                                grid[pos.x, pos.y] = null;
                                changed = true;
                            }
                        }
                    }
                }

            } while (changed);
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

                    var block = grid[x, y];
                    CollectConnected(x, y, block, grid, visited, out var connected);

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
    
        private void CollectConnected(int x, int y, PlayableBlockPresenter block, PlayableBlockPresenter[,] grid, bool[,] visited, out List<(int x, int y)> connected)
        {
            connected = new List<(int x, int y)>();
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((x, y));

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                if (cx < 0 || cx >= width || cy < 0 || cy >= height)
                    continue;

                var current = grid[cx, cy];

                if (visited[cx, cy] || current == null || current.Model.GroupId != block.Model.GroupId
                    || current.Model.State == BlockState.Falling || current.Model.State == BlockState.Destroying)
                    continue;

                visited[cx, cy] = true;
                connected.Add((cx, cy));

                queue.Enqueue((cx + 1, cy));
                queue.Enqueue((cx - 1, cy));
                queue.Enqueue((cx, cy + 1));
                queue.Enqueue((cx, cy - 1));
            }
        }

        private PlayableBlockPresenter[,] BuildGrid(int width, int height)
        {
            var grid = new PlayableBlockPresenter[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    grid[x, y] = ActiveColumns[x].Cells[y].PlayableBlockPresenter;
                }
            }
            return grid;
        }

        private void MarkHorizontalLines(PlayableBlockPresenter[,] grid, bool[,] lineMarks, int width, int height)
        {
            for (var y = 0; y < height; y++)
            {
                var count = 1;
                for (var x = 1; x < width; x++)
                {
                    var current = grid[x, y];
                    var previous = grid[x - 1, y];

                    if (current == null || previous == null 
                        || current.Model.State == BlockState.Falling 
                        || current.Model.State == BlockState.Destroying
                        || previous.Model.State == BlockState.Falling
                        || previous.Model.State == BlockState.Destroying)
                    {
                        count = 1;
                        continue;
                    }

                    if (current.Model.GroupId == previous.Model.GroupId && current.Model.GroupId != -1)
                        count++;
                    else
                        count = 1;

                    if (count < 3) continue;
                    
                    for (var k = 0; k < count; k++)
                    {
                        lineMarks[x - k, y] = true;
                    }
                }
            }
        }

        private void MarkVerticalLines(PlayableBlockPresenter[,] grid, bool[,] lineMarks, int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                var count = 1;
                for (var y = 1; y < height; y++)
                {
                    var current = grid[x, y];
                    var previous = grid[x, y - 1];

                    if (current == null || previous == null 
                        || current.Model.State == BlockState.Falling 
                        || current.Model.State == BlockState.Destroying
                        || previous.Model.State == BlockState.Falling
                        || previous.Model.State == BlockState.Destroying)
                    {
                        count = 1;
                        continue;
                    }

                    if (current.Model.GroupId == previous.Model.GroupId && current.Model.GroupId != -1)
                        count++;
                    else
                        count = 1;

                    if (count < 3) continue;
                    for (var k = 0; k < count; k++)
                    {
                        lineMarks[x, y - k] = true;
                    }
                }
            }
        }
        
        private async UniTask<bool> NormalizeGameZone(CancellationToken token)
        {
            bool moved = false;
            var fallTasks = new List<UniTask>();

            foreach (var column in ActiveColumns)
            {
                int emptyCount = 0;

                for (int row = 0; row < column.Cells.Count; row++)
                {
                    var cell = column.Cells[row];
                    var block = cell.PlayableBlockPresenter;

                    if (block == null)
                    {
                        emptyCount++;
                        continue;
                    }

                    if (block.Model.State == BlockState.Destroying)
                        continue;

                    if (emptyCount > 0)
                    {
                        var targetCell = column.Cells[row - emptyCount];
                        
                        cell.PlayableBlockPresenter = null;
                        targetCell.PlayableBlockPresenter = block;
                        block.transform.SetParent(targetCell.transform, true);
                        block.Model.State = BlockState.Falling;

                        fallTasks.Add(MoveBlockAnim(block, targetCell, token));
                        moved = true;
                    }
                }
            }

            if (fallTasks.Count > 0)
                await UniTask.WhenAll(fallTasks).AttachExternalCancellation(token);

            foreach (var column in ActiveColumns)
            {
                foreach (var cell in column.Cells)
                {
                    var block = cell.PlayableBlockPresenter;
                    if (block != null && block.Model.State == BlockState.Falling)
                        block.Model.State = BlockState.Idle;
                }
            }

            return moved;
        }

        private async UniTask MoveBlockAnim(PlayableBlockPresenter block, CellController targetCell, CancellationToken token)
        {
            block.transform.DOComplete(true);
            using var reg = token.Register(() => block.transform.DOComplete(true));

            token.ThrowIfCancellationRequested();
            await block.transform.DOMove(targetCell.transform.position, 0.25f).WithCancellation(token);
        }

        public void SubscribeToActiveCells(Action<PointerEventData, CellController> onBeginDrag,
            Action<PointerEventData, CellController> onEndDrag)
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

        public void UnsubscribeFromAllCells(Action<PointerEventData, CellController> onBeginDrag,
            Action<PointerEventData, CellController> onEndDrag)
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