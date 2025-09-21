using System;
using _Project.Scripts.Interfaces;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using VContainer;

namespace _Project.Scripts.Services
{
    public class ResetLevelService
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        public void ResetLevel()
        {
            var returnToPoolList = _objectsRegistry.GetAllByInterface<IReturnToPool>();
            returnToPoolList.ForEach(obj => obj.ReturnToPool());
            
            var disposeList = _objectsRegistry.GetAllByInterface<IDisposable>();
            disposeList.ForEach(obj => obj.Dispose());
            
            _objectsRegistry.GetTypedList<PlayableBlockPresenter>().Clear();
        }
    }
}