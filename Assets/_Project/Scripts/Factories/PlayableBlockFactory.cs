using _Project.Scripts.SO;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.Factories
{
    public class PlayableBlockFactory
    {
        [Inject] private IObjectResolver _resolver;
        [Inject] private PlayableBlocksConfig _playableBlocksConfig;
        
        public async UniTask<T> CreatePlayableBlock<T>(Transform parent, int groupId) where T : PlayableBlockPresenter
        {
            PlayableBlockPresenter playableBlockPresenter = null;
            foreach (var prefab in _playableBlocksConfig.ListPlayableBlockPrefabs)
            {
                if (prefab is T tPrefab && prefab.Model.GroupId == groupId)
                {
                    playableBlockPresenter = _resolver.Instantiate(tPrefab, parent);
                    await UniTask.Yield();
                    playableBlockPresenter.Initialize();
                    break;
                }
            }

            return (T)playableBlockPresenter;
        }
    }
}