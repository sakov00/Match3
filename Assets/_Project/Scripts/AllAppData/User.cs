using UniRx;

namespace _Project.Scripts.AllAppData
{
    public class User
    {
        private readonly IntReactiveProperty _currentLevel = new (0);
        
        public IReactiveProperty<int> CurrentLevelReactive => _currentLevel;
        
        public int CurrentLevel
        {
            get => _currentLevel.Value;
            set => _currentLevel.Value = value;
        }
        
        public void Dispose()
        {
            _currentLevel?.Dispose();
        }
    }
}