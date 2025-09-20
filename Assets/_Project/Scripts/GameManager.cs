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
    public class GameManager : IInitializable, IDisposable
    {
        [Inject] protected AppData AppData;
        [Inject] private WindowsManager _windowsManager;
        [Inject] private ResetLevelService _resetLevelService;
        [Inject] private FileLevelManager _fileLevelManager;
        [Inject] private PlayableBlockPool _playableBlockPool;
        [Inject] private ApplicationEventsHandler _applicationEventsHandler;
        
        public virtual void Initialize()
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;
            StartLevel(AppData.User.CurrentLevel).Forget();
        }

        public virtual async UniTask StartLevel(int levelIndex)
        {
            Dispose();
            await _windowsManager.ShowWindow<LoadingWindowPresenter>();
            var gameWindow = _windowsManager.GetWindow<GameWindowPresenter>();
            gameWindow.Dispose();
            gameWindow.ShowFast();
            AppData.LevelEvents.Dispose();
            AppData.LevelEvents.Initialize();
            AppData.User.CurrentLevel = levelIndex;
            _resetLevelService.ResetLevel();
            await _fileLevelManager.LoadLevel(levelIndex);
            _windowsManager.GetWindow<GameWindowPresenter>().Initialize();
            _applicationEventsHandler.OnApplicationQuited += OnApplicationQuit;
            _applicationEventsHandler.OnApplicationPaused += OnApplicationPause;
            _windowsManager.HideWindow<LoadingWindowPresenter>();
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