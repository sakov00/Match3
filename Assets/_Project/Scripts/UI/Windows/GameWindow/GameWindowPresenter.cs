using _Project.Scripts.AllAppData;
using _Project.Scripts.Registries;
using _Project.Scripts.Services;
using _Project.Scripts.UI.Windows.BaseWindow;
using _Project.Scripts.UI.Windows.LoadingWindow;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.Windows.GameWindow
{
    public class GameWindowPresenter : BaseWindowPresenter
    {
        [Inject] private AppData _appData;
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private GameManager _gameManager;
        
        [SerializeField] private GameWindowModel _model;
        [SerializeField] private GameWindowView _view;
        
        protected override BaseWindowModel BaseModel => _model;
        protected override BaseWindowView BaseView => _view;

        public ReactiveCommand NextLevelCommand { get; } = new();
        public ReactiveCommand RestartLevelCommand { get; } = new();

        protected override void Awake()
        {
            base.Awake();
            NextLevelCommand.Subscribe(_ => NextLevel().Forget()).AddTo(this);
            RestartLevelCommand.Subscribe(_ => RestartLevel().Forget()).AddTo(this);
        }

        public void Initialize()
        {
            _appData.LevelEvents.WinEvent += WinHandle;
        }

        private async UniTaskVoid NextLevel()
        {
            WindowsManager.ShowWindow<LoadingWindowPresenter>();
            _appData.User.CurrentLevel =+ 1;
            await _gameManager.StartLevel(_appData.User.CurrentLevel);
            WindowsManager.HideWindow<LoadingWindowPresenter>();
        }
        
        private async UniTaskVoid RestartLevel()
        {
            WindowsManager.ShowWindow<LoadingWindowPresenter>();
            await _gameManager.StartLevel(_appData.User.CurrentLevel);
            WindowsManager.HideWindow<LoadingWindowPresenter>();
        }

        private async void WinHandle()
        {

        }

        public void Dispose()
        {
            _appData.LevelEvents.WinEvent -= WinHandle;
            _view.Dispose();
        }
    }
}