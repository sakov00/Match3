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
                .Subscribe(_ => AllBlockDestroyed())
                .AddTo(_disposables);
        }

        private void AllBlockDestroyed()
        {
            WinEvent?.Invoke();
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}