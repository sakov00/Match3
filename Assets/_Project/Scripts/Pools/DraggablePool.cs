using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Factories;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.DraggableObjects.Draggable;
using UnityEngine;
using  VContainer;

namespace _Project.Scripts.Pools
{
    public class DraggablePool
    {
        [Inject] private DraggableFactory _draggableFactory;
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        private Transform _containerTransform;
        private readonly List<DraggablePresenter> _availableDraggables = new();

        public void SetContainer(Transform transform)
        {
            _containerTransform = transform;
        }
        
        public List<DraggablePresenter> GetAvailableDraggables() => _availableDraggables;
        
        public T Get<T>(Transform parent, Vector2 anchoredPos = default, Quaternion rotation = default) where T : DraggablePresenter
        {
            var draggable = _availableDraggables.OfType<T>().FirstOrDefault();

            if (draggable != null)
            {
                _availableDraggables.Remove(draggable);
                draggable.transform.position = anchoredPos;
                draggable.transform.rotation = rotation;
                draggable.gameObject.SetActive(true);
                draggable.transform.SetParent(parent, false);
            }
            else
            {
                draggable = _draggableFactory.CreateDraggable<T>(parent, anchoredPos, rotation);
            }
            
            _objectsRegistry.Register(draggable);
            return draggable;
        }

        public void Return<T>(T draggable) where T : DraggablePresenter
        {
            if (!_availableDraggables.Contains(draggable))
            {
                _availableDraggables.Add(draggable);
            }
            
            draggable.gameObject.SetActive(false);
            draggable.transform.SetParent(_containerTransform, false); 
            _objectsRegistry.Unregister(draggable);
        }
        
        public void Remove<T>(T draggable) where T : DraggablePresenter
        {
            if (!_availableDraggables.Contains(draggable))
            {
                _availableDraggables.Remove(draggable);
            }
        }
    }
}