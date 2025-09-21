using System;
using System.Threading;
using _Project.Scripts._VContainer;
using _Project.Scripts.Enums;
using _Project.Scripts.Interfaces;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    public class PlayableBlockPresenter : MonoBehaviour, IReturnToPool, IDisposable
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private PlayableBlockPool _playableBlockPool;
        
        [field:SerializeField] public PlayableBlockModel Model { get; protected set; }
        [field:SerializeField] public PlayableBlockView View { get; protected set; }
        
        public bool IsInteractable => Model.State == BlockState.Idle;

        public virtual void Initialize()
        {
            InjectManager.Inject(this);
            _objectsRegistry.Register(this);
            Model.State = BlockState.Idle;
            View.IdleAnim().Forget();
        }

        public async UniTask DestroyAnimStart(CancellationToken token)
        {
            Model.State = BlockState.Destroying;
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