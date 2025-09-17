using _Project.Scripts._GlobalLogic;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.AllAppData
{
    public class User
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly IntReactiveProperty _currentLevel;
        
        public IReactiveProperty<int> CurrentLevelReactive => _currentLevel;
        
        public int CurrentLevel
        {
            get => _currentLevel.Value;
            set => _currentLevel.Value = value;
        }
        
        public User()
        {
            int savedLevel = PlayerPrefs.GetInt(GameConstants.PrefKeys.CurrentLevel, 0);
            _currentLevel = new IntReactiveProperty(savedLevel);

            _currentLevel
                .Skip(1)
                .Subscribe(SaveLevel)
                .AddTo(_disposables);
        }

        private void SaveLevel(int level)
        {
            PlayerPrefs.SetInt(GameConstants.PrefKeys.CurrentLevel, level);
            PlayerPrefs.Save();
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _currentLevel?.Dispose();
        }
    }
}