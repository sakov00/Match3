using System;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.AllAppData
{
    public class LevelEvents : IInitializable, IDisposable
    {
        [Inject] private ObjectsRegistry _objectsRegistry;

        public event Action WinEvent;

        private CompositeDisposable _disposables;

        public void Initialize()
        {
            InjectManager.Inject(this);
            _disposables = new CompositeDisposable();
            _objectsRegistry
                .GetTypedList<PlayableBlockPresenter>()
                .ObserveRemove()
                .Subscribe(_ => CheckAllBlocksDestroyed())
                .AddTo(_disposables);
        }

        private void CheckAllBlocksDestroyed()
        {
            if(_objectsRegistry.GetTypedList<PlayableBlockPresenter>().Count == 0)
                WinEvent?.Invoke();
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}