using System;
using System.Threading;
using _Project.Scripts._VContainer;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    public class PlayableBlockPresenter : MonoBehaviour
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private PlayableBlockPool _playableBlockPool;
        
        [field:SerializeField] public PlayableBlockModel Model { get; protected set; }
        [field:SerializeField] public PlayableBlockView View { get; protected set; }
        
        public virtual void Initialize()
        {
            InjectManager.Inject(this);
            _objectsRegistry.Register(this);
            View.StartIdleAnim().Forget();
        }

        public async UniTask DestroyAnimStart(CancellationToken token)
        {
            await View.DestroyAnim().AttachExternalCancellation(token);
            ReturnToPool();
        }

        public virtual void ReturnToPool()
        {
            _playableBlockPool.Return(this);
            Dispose();
        }

        public virtual void Dispose()
        {
            View.Dispose();
            _objectsRegistry.Unregister(this);
        }
    }
}