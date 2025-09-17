using _Project.Scripts._VContainer;
using _Project.Scripts.Pools;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.Services
{
    public class PoolsManager : MonoBehaviour
    {
        [SerializeField] private Transform _draggablePoolContainer;
        
        [Inject] private PlayableBlockPool _playableBlockPool;

        private void Start()
        {
            InjectManager.Inject(this);
            _playableBlockPool.SetContainer(_draggablePoolContainer);
        }
    }
}