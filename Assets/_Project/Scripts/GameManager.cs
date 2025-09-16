using System.Threading;
using System.Threading.Tasks;
using _Project.Scripts._VContainer;
using _Project.Scripts.AllAppData;
using _Project.Scripts.Pools;
using _Project.Scripts.UI.Windows;
using _Project.Scripts.UI.Windows.GameWindow;
using _Project.Scripts.UI.Windows.LoadingWindow;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts
{
    public class GameManager : IAsyncStartable
    {
        [Inject] protected AppData AppData;
        [Inject] private WindowsManager _windowsManager;
        //[Inject] private LevelManager _levelManager;
        [Inject] private DraggablePool _draggablePool;
        
        public virtual async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            InjectManager.Inject(this);
            
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;
            
            await _windowsManager.ShowWindow<LoadingWindowPresenter>();
            await UniTask.Delay(2000, cancellationToken: cancellation);
            var gameWindow = _windowsManager.GetWindow<GameWindowPresenter>();
            gameWindow.Initialize();
            gameWindow.ShowFast();
            // await _levelManager.LoadLevel();
             _windowsManager.GetWindow<GameWindowPresenter>();
             _windowsManager.HideWindow<LoadingWindowPresenter>();
        }

        public virtual async UniTask StartLevel(int levelIndex)
        {
            _windowsManager.GetWindow<GameWindowPresenter>().Dispose();
            _windowsManager.ShowWindow<GameWindowPresenter>();
            AppData.LevelEvents.Dispose();
            AppData.LevelEvents.Initialize();
            AppData.User.CurrentLevel = levelIndex;
            //await LoadLevel(levelIndex);
            _windowsManager.GetWindow<GameWindowPresenter>().Initialize();
        }
        
        private void OnApplicationQuit()
        {
            //_levelManager?.SaveLevel().Forget();
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                //_levelManager?.SaveLevel().Forget();
            }
        }
    }
}