using System;
using System.Threading;
using System.Threading.Tasks;
using _Project.Scripts._VContainer;
using _Project.Scripts.AllAppData;
using _Project.Scripts.Pools;
using _Project.Scripts.Services;
using _Project.Scripts.UI.Windows;
using _Project.Scripts.UI.Windows.GameWindow;
using _Project.Scripts.UI.Windows.LoadingWindow;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts
{
    public class GameManager : IAsyncStartable, IDisposable
    {
        [Inject] protected AppData AppData;
        [Inject] private WindowsManager _windowsManager;
        [Inject] private ResetLevelService _resetLevelService;
        [Inject] private FileLevelManager _fileLevelManager;
        [Inject] private PlayableBlockPool _playableBlockPool;
        [Inject] private ApplicationEventsHandler _applicationEventsHandler;
        private IAsyncStartable _asyncStartableImplementation;

        public virtual async UniTask StartAsync(CancellationToken cancellation = default)
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;
            var loadingWindow = _windowsManager.GetWindow<LoadingWindowPresenter>();
            loadingWindow.ShowFast();
            var gameWindow = _windowsManager.GetWindow<GameWindowPresenter>();
            await _fileLevelManager.Initialize();
            await StartLevel(AppData.User.CurrentLevel);
            await _windowsManager.HideWindow<LoadingWindowPresenter>();
        }

        public virtual async UniTask StartLevel(int levelIndex)
        {
            Dispose();
            AppData.LevelEvents.Dispose();
            AppData.LevelEvents.Initialize();
            AppData.User.CurrentLevel = levelIndex;
            _resetLevelService.ResetLevel();
            await _fileLevelManager.LoadLevel(levelIndex);
            _windowsManager.GetWindow<GameWindowPresenter>().Initialize();
            _applicationEventsHandler.OnApplicationQuited += OnApplicationQuit;
            _applicationEventsHandler.OnApplicationPaused += OnApplicationPause;
        }
        
        private void OnApplicationQuit()
        {
            _fileLevelManager?.SaveLevelProgress(AppData.User.CurrentLevel).Forget();
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
                _fileLevelManager?.SaveLevelProgress(AppData.User.CurrentLevel).Forget();
        }

        public void Dispose()
        {
            _applicationEventsHandler.OnApplicationQuited -= OnApplicationQuit;
            _applicationEventsHandler.OnApplicationPaused -= OnApplicationPause;
        }
    }
}