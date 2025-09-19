using System.Threading;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public class PlayerBlockMover
    {
        private readonly float _duration = 0.25f;

        public async UniTask MoveToEmptyCell(CellController oldCell, CellController targetCell, Vector2Int direction, CancellationToken token)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;
            
            oldBlock.transform.DOComplete(true);
            token.Register(() => oldBlock.transform.DOComplete(true));
            
            if (direction == Vector2Int.up || direction == Vector2Int.right || direction == Vector2Int.left)
                oldBlock.transform.SetParent(targetCell.transform, true);

            oldCell.PlayableBlockPresenter = null;
            targetCell.PlayableBlockPresenter = oldBlock;

            var tween = oldBlock.transform.DOMove(targetCell.transform.position, _duration);
            tween.OnComplete(() =>
            {
                if (direction == Vector2Int.down) oldBlock.transform.SetParent(targetCell.transform, true);
            });
            await tween.Play();
        }

        public async UniTask SwapBlocks(CellController oldCell, CellController targetCell, Vector2Int direction, CancellationToken token)
        {
            var oldBlock = oldCell.PlayableBlockPresenter;
            var targetBlock = targetCell.PlayableBlockPresenter;

            oldBlock.transform.DOComplete(true);
            targetBlock.transform.DOComplete(true);

            token.Register(() => oldBlock.transform.DOComplete(true));
            token.Register(() => targetBlock.transform.DOComplete(true));
            
            if (direction == Vector2Int.up) oldBlock.transform.SetParent(targetCell.transform, true);
            if (direction == Vector2Int.down) targetBlock.transform.SetParent(oldCell.transform, true);
            if (direction == Vector2Int.left || direction == Vector2Int.right)
            {
                oldBlock.transform.SetParent(targetCell.transform, true);
                targetBlock.transform.SetParent(oldCell.transform, true);
            }

            oldCell.PlayableBlockPresenter = targetBlock;
            targetCell.PlayableBlockPresenter = oldBlock;

            var oldTweenTask = oldBlock.transform.DOMove(targetCell.transform.position, _duration).OnComplete(() =>
            {
                if (direction == Vector2Int.down) oldBlock.transform.SetParent(targetCell.transform, true);
            }).ToUniTask();
            var targetTweenTask = targetBlock.transform.DOMove(oldCell.transform.position, _duration).OnComplete(() =>
            {
                if (direction == Vector2Int.up) targetBlock.transform.SetParent(oldCell.transform, true);
            }).ToUniTask();

            await UniTask.WhenAll(oldTweenTask, targetTweenTask);
        }
    }
}
