using System;
using UnityEngine;

namespace _Project.Scripts
{
    public class ApplicationEventsHandler : MonoBehaviour
    {
        public event Action OnApplicationQuited;
        public event Action<bool> OnApplicationPaused;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnApplicationQuit()
        {
            OnApplicationQuited?.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
                OnApplicationPaused?.Invoke(pause);
        }
    }
}