using _Project.Scripts._VContainer;
using _Project.Scripts.Pools;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.Services
{
    public class PoolsManager : MonoBehaviour
    {
        [SerializeField] private Transform _draggablePoolContainer;
        
        [Inject] private DraggablePool _draggablePool;

        private void Start()
        {
            InjectManager.Inject(this);
            _draggablePool.SetContainer(_draggablePoolContainer);
        }
    }
}