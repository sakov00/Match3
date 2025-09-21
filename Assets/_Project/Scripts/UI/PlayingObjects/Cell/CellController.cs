using System;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects.Cell
{
    public class CellController : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDisposable
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [field:SerializeField] public CellModel Model { get; private set; }
        [field:SerializeField] public PlayableBlockPresenter PlayableBlockPresenter { get; set; }
        
        public event Action<PointerEventData, CellController> OnBeginedDrag;
        public event Action<PointerEventData, CellController> OnEndedDrag;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PlayableBlockPresenter == null)
            {
                PlayableBlockPresenter = GetComponentInChildren<PlayableBlockPresenter>();
                if (PlayableBlockPresenter != null)
                {
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif

        private void Awake()
        {
            InjectManager.Inject(this);
            Initialize();
        }
        
        public void Initialize()
        {
            _objectsRegistry.Register(this);
        }
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            
        }
        
        public virtual void OnDrag(PointerEventData eventData)
        {
            
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginedDrag?.Invoke(eventData, this);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            OnEndedDrag?.Invoke(eventData, this);
        }
        
        public virtual CellModel GetSavableModel()
        {
            if (PlayableBlockPresenter == null) 
                return null;
            
            Model.PlayableBlockModel = PlayableBlockPresenter.Model;
            return Model;
        }

        public virtual void SetSavableModel(CellModel savableModel)
        {
            Model = savableModel;
        }

        public void Dispose()
        {
            OnBeginedDrag = null;
            OnEndedDrag = null;
            PlayableBlockPresenter = null;
            Model.PlayableBlockModel = null;
        }
    }
}