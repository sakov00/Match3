using System;
using _Project.Scripts._VContainer;
using _Project.Scripts.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.DraggableObjects.Draggable
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class DraggablePresenter : MonoBehaviour, IDisposable, ISavableLogic
    {
        [field: NonSerialized] public Vector3 RealPos { get; set; }
        
        public abstract DraggableModel BaseModel { get; }
        public abstract DraggableView BaseView { get; }

        private void Start()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            InjectManager.Inject(this);
        }
        public abstract ISavableModel GetSavableModel();
        public abstract void SetSavableModel(ISavableModel savableModel);
        protected abstract void ReturnToPool();
        public abstract void Dispose();
    }
}
