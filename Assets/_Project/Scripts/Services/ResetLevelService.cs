using System;
using _Project.Scripts.AllAppData;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using VContainer;

namespace _Project.Scripts.Services
{
    public class ResetLevelService
    {
        [Inject] private AppData _appData;
        [Inject] private PlayableBlockPool _playableBlockPool;
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        public void ResetLevel()
        {
            foreach (var obj in _objectsRegistry.GetAllByInterface<IDisposable>())
            {
                obj.Dispose();
            }
            
            _objectsRegistry.Clear();
        }
    }
}