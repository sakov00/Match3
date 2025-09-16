using _Project.Scripts.AllAppData;
using _Project.Scripts.Registries;
using _Project.Scripts.Services;
using _Project.Scripts.UI.Windows.BaseWindow;
using _Project.Scripts.UI.Windows.LoadingWindow;
using UniRx;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.Windows.GameWindow
{
    public class GameWindowPresenter : BaseWindowPresenter
    {
        [Inject] private AppData _appData;
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private ResetLevelService _resetLevelService;
        
        [SerializeField] private GameWindowModel _model;
        [SerializeField] private GameWindowView _view;
        
        protected override BaseWindowModel BaseModel => _model;
        protected override BaseWindowView BaseView => _view;

        public ReactiveCommand NextLevelCommand { get; } = new();
        public ReactiveCommand RestartLevelCommand { get; } = new();

        protected override void Awake()
        {
            // base.Awake();
            // NextLevelCommand.Subscribe(_ => NextLevel()).AddTo(this);
            // RestartLevelCommand.Subscribe(_ => RestartLevel()).AddTo(this);
        }

        public void Initialize()
        {
            // _appData.LevelEvents.WinEvent += WinHandle;
        }

        private void NextLevel()
        {
            WindowsManager.ShowWindow<LoadingWindowPresenter>();
        }
        
        private void RestartLevel()
        {
            WindowsManager.ShowWindow<LoadingWindowPresenter>();
        }

        private async void WinHandle()
        {

        }

        private void OnDestroy() => Dispose();

        public void Dispose()
        {
            // _appData.LevelEvents.WinEvent -= WinHandle;
        }
    }
}