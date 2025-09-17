using _Project.Scripts.Extensions;
using _Project.Scripts.Interfaces;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.DraggableObjects.Draggable;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.DraggableObjects.PlayableBlock
{
    public class PlayableBlockPresenter : DraggablePresenter
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private PlayableBlockPool _playableBlockPool;
        
        [field:SerializeField] public PlayableBlockModel Model { get; protected set; }
        [field:SerializeField] public PlayableBlockView View { get; protected set; }

        public override DraggableModel BaseModel => Model;
        public override DraggableView BaseView => View;
        
        public override void Initialize()
        {
            base.Initialize();
            _objectsRegistry.Register(this);
            View.Initialize(this);
        }
        
        public override ISavableModel GetSavableModel()
        {
            Model.SaveAnchoredPosition = View.RectTransform.anchoredPosition;
            Model.SaveRotation = View.RectTransform.rotation;
            Model.ParentPath = View.RectTransform.parent?.GetFullPath();
            return Model;
        }

        public override void SetSavableModel(ISavableModel savableModel)
        {
            if (savableModel is not PlayableBlockModel playableBlockModel) return;
            Model = playableBlockModel;
            View.RectTransform.anchoredPosition = playableBlockModel.SaveAnchoredPosition;
            View.RectTransform.rotation = playableBlockModel.SaveRotation;
        }

        protected override void ReturnToPool()
        {
            _playableBlockPool.Return(this);
            Dispose();
        }

        public override void Dispose()
        {
            View.Dispose();
            _objectsRegistry.Unregister(this);
        }
    }
}