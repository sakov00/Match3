using System;
using VContainer.Unity;

namespace _Project.Scripts.AllAppData
{
    public class AppData : IDisposable
    {
        public User User { get; private set; }
        public LevelEvents LevelEvents { get; private set; }

        public AppData()
        {
            User = new User();
            LevelEvents = new LevelEvents();
        }

        public void Dispose()
        {
            User.Dispose();
            LevelEvents.Dispose();
        }
    }
}