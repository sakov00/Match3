using System.Threading;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public class BlockMover
    {
        private readonly float _duration = 0.25f;

        public async UniTask MoveToEmptyCell(CellController oldCell, CellController targetCell, Vector2Int direction, CancellationToken token)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;
            var duration = 0.25f;

            if (direction == Vector2Int.up)
                oldBlock.transform.SetParent(targetCell.transform, true);
            if (direction == Vector2Int.right || direction == Vector2Int.left)
                oldBlock.transform.SetParent(targetCell.transform, true);
            
            await oldBlock.transform.DOMove(targetCell.transform.position, duration)
                .Play().WithCancellation(token);
            
            if (direction == Vector2Int.down)
                oldBlock.transform.SetParent(targetCell.transform, true);

            oldCell.PlayableBlockPresenter = null;
            targetCell.PlayableBlockPresenter = oldBlock;
        }

        public async UniTask SwapBlocks(CellController oldCell, CellController targetCell, Vector2Int direction, CancellationToken token)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;
            var targetBlock = targetCell.PlayableBlockPresenter;

            if (direction == Vector2Int.up) oldBlock.transform.SetParent(targetCell.transform, true);
            if (direction == Vector2Int.down) targetBlock.transform.SetParent(oldCell.transform, true);
            if (direction == Vector2Int.left || direction == Vector2Int.right)
            {
                oldBlock.transform.SetParent(targetCell.transform, true);
                targetBlock.transform.SetParent(oldCell.transform, true);
            }

            var sequence = DOTween.Sequence();
            sequence.Append(oldBlock.transform.DOMove(targetCell.transform.position, _duration));
            sequence.Join(targetBlock.transform.DOMove(oldCell.transform.position, _duration));
            await sequence.Play().WithCancellation(token);

            if (direction == Vector2Int.up) targetBlock.transform.SetParent(oldCell.transform, true);
            if (direction == Vector2Int.down) oldBlock.transform.SetParent(targetCell.transform, true);

            oldCell.PlayableBlockPresenter = targetBlock;
            targetCell.PlayableBlockPresenter = oldBlock;
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
    }
}
