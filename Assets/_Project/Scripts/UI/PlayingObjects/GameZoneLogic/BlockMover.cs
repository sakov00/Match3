using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public class BlockMover
    {
        private readonly float _duration = 0.25f;

        public async UniTask MoveToEmptyCell(CellController oldCell, CellController targetCell, Vector2Int direction)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;

            oldBlock.transform.SetParent(targetCell.transform, true);
            await oldBlock.transform.DOMove(targetCell.transform.position, _duration).Play();

            oldCell.PlayableBlockPresenter = null;
            targetCell.PlayableBlockPresenter = oldBlock;
        }

        public async UniTask SwapBlocks(CellController oldCell, CellController targetCell, Vector2Int direction)
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
            await sequence.Play();

            if (direction == Vector2Int.up) targetBlock.transform.SetParent(oldCell.transform, true);
            if (direction == Vector2Int.down) oldBlock.transform.SetParent(targetCell.transform, true);

            oldCell.PlayableBlockPresenter = targetBlock;
            targetCell.PlayableBlockPresenter = oldBlock;
        }

        public async UniTask DropBlockDown(CellController cell, Column column)
        {
            var block = cell.PlayableBlockPresenter;
            int rowIndex = cell.Model.RowIndex;

            CellController lowestEmptyCell = null;
            for (int row = rowIndex - 1; row >= 0; row--)
            {
                if (column.Cells[row].PlayableBlockPresenter == null)
                    lowestEmptyCell = column.Cells[row];
                else
                    break;
            }

            if (lowestEmptyCell == null) return;

            cell.PlayableBlockPresenter = null;
            lowestEmptyCell.PlayableBlockPresenter = block;
            block.transform.SetParent(lowestEmptyCell.transform, true);
            await block.transform.DOMove(lowestEmptyCell.transform.position, _duration).Play();
        }
    }
}
