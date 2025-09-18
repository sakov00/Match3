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
            var draggable = _availablePlayableBlocks.OfType<T>().FirstOrDefault(x => x.Model.GroupId == groupId);

            if (draggable != null)
            {
                _availablePlayableBlocks.Remove(draggable);
                draggable.gameObject.SetActive(true);
                draggable.transform.SetParent(parent, false);
            }
            else
            {
                draggable = await _playableBlockFactory.CreatePlayableBlock<T>(parent, groupId);
            }
            
            _objectsRegistry.Register(draggable);
            return draggable;
        }

        public void Return<T>(T draggable) where T : PlayableBlockPresenter
        {
            if (!_availablePlayableBlocks.Contains(draggable))
            {
                _availablePlayableBlocks.Add(draggable);
            }
            
            draggable.gameObject.SetActive(false);
            draggable.transform.SetParent(_containerTransform, false); 
            _objectsRegistry.Unregister(draggable);
        }
        
        public void Remove<T>(T draggable) where T : PlayableBlockPresenter
        {
            if (!_availablePlayableBlocks.Contains(draggable))
            {
                _availablePlayableBlocks.Remove(draggable);
            }
        }
    }
}