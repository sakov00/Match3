using System;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Scripts._GlobalLogic
{
    public class GameTimer : ITickable
    {
        public event Action OnEverySecond;

        private float _time = 0;

        public void Tick()
        {
            _time += Time.deltaTime;
            if (_time >= 1f)
            {
                _time -= 1f;
                OnEverySecond?.Invoke();
            }
        }
    }
}