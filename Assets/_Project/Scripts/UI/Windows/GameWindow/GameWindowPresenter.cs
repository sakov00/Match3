using System;
using _Project.Scripts.AllAppData;
using _Project.Scripts.Enums;
using _Project.Scripts.Registries;
using _Project.Scripts.Services;
using _Project.Scripts.UI.Windows.BaseWindow;
using _Project.Scripts.UI.Windows.LoadingWindow;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using VContainer;
using GameZone = _Project.Scripts.UI.PlayingObjects.GameZoneLogic.GameZone;

namespace _Project.Scripts.UI.Windows.GameWindow
{
    public class GameWindowPresenter : BaseWindowPresenter
    {
        [Inject] private AppData _appData;
        [Inject] private FileLevelManager _fileLevelManager;
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private GameManager _gameManager;
        [Inject] private WindowsManager _windowsManager;
        
        [SerializeField] private GameWindowModel _model;
        [SerializeField] private GameWindowView _view;
        [SerializeField] private GameZone _gameZone;

        public override WindowType WindowType => _model.WindowType;
        public ReactiveCommand NextLevelCommand { get; } = new();
        public ReactiveCommand RestartLevelCommand { get; } = new();
        
        private CompositeDisposable _disposables;

        public void Initialize()
        {
            _disposables = new CompositeDisposable();
            NextLevelCommand.Subscribe(_ => NextLevel().Forget()).AddTo(_disposables);
            RestartLevelCommand.Subscribe(_ => RestartLevel().Forget()).AddTo(_disposables);
            _appData.LevelEvents.WinEvent += WinHandle;
            _view.Initialize();
            _gameZone.Initialize();
        }

        private async UniTaskVoid NextLevel()
        {
            Dispose();
            _appData.User.CurrentLevel += 1;
            await _windowsManager.ShowWindow<LoadingWindowPresenter>();
            _view.Dispose();
            await _gameManager.StartLevel(_appData.User.CurrentLevel);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            await _windowsManager.HideWindow<LoadingWindowPresenter>();
        }
        
        private async UniTaskVoid RestartLevel()
        {
            Dispose();
            _fileLevelManager.RemoveProgress(_appData.User.CurrentLevel);
            await _windowsManager.ShowWindow<LoadingWindowPresenter>();
            _view.Dispose();
            await _gameManager.StartLevel(_appData.User.CurrentLevel);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            await _windowsManager.HideWindow<LoadingWindowPresenter>();
        }

        private void WinHandle()
        {
            NextLevel().Forget();
        }
        
        public override Tween Show() => _view.Show();
        public override Tween Hide() => _view.Hide();
        public override void ShowFast() => _view.ShowFast();
        public override void HideFast() => _view.HideFast();

        public void Dispose()
        {
            _disposables?.Dispose();
            _appData.LevelEvents.WinEvent -= WinHandle;
            _gameZone.Dispose();
        }
    }
}