using System;
using VContainer.Unity;

namespace _Project.Scripts.AllAppData
{
    public class AppData : IInitializable, IDisposable
    {
        public LevelEvents LevelEvents { get; private set; }
        public User User { get; private set; }

        public void Initialize()
        {
            LevelEvents = new LevelEvents();
            User = new User();
        }

        public void Dispose()
        {
            LevelEvents.Dispose();
        }
    }
}