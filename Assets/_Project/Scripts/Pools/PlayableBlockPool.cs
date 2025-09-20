using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Factories;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using Cysharp.Threading.Tasks;
using UnityEngine;
using  VContainer;

namespace _Project.Scripts.Pools
{
    public class PlayableBlockPool
    {
        [Inject] private PlayableBlockFactory _playableBlockFactory;
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        private Transform _containerTransform;
        private readonly List<PlayableBlockPresenter> _availablePlayableBlocks = new();

        public void SetContainer(Transform transform)
        {
            _containerTransform = transform;
        }
        
        public List<PlayableBlockPresenter> GetAvailablePlayableBlocks() => _availablePlayableBlocks;
        
        public async UniTask<T> Get<T>(Transform parent, int groupId) where T : PlayableBlockPresenter
        {
            var playableBlock = _availablePlayableBlocks.OfType<T>().FirstOrDefault(x => x.Model.GroupId == groupId);

            if (playableBlock != null)
            {
                _availablePlayableBlocks.Remove(playableBlock);
                playableBlock.gameObject.SetActive(true);
                playableBlock.transform.SetParent(parent, false);
            }
            else
            {
                playableBlock = await _playableBlockFactory.CreatePlayableBlock<T>(parent, groupId);
            }
            
            playableBlock.Initialize();
            return playableBlock;
        }

        public void Return<T>(T playableBlock) where T : PlayableBlockPresenter
        {
            if (playableBlock == null) return;
            
            if (!_availablePlayableBlocks.Contains(playableBlock))
            {
                _availablePlayableBlocks.Add(playableBlock);
            }
            
            playableBlock.gameObject.SetActive(false);
            playableBlock.transform.SetParent(_containerTransform, false); 
            _objectsRegistry.Unregister(playableBlock);
        }
        
        public void Remove<T>(T playableBlock) where T : PlayableBlockPresenter
        {
            if (!_availablePlayableBlocks.Contains(playableBlock))
            {
                _availablePlayableBlocks.Remove(playableBlock);
            }
        }
    }
}