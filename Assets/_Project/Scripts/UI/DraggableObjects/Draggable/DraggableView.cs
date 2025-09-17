using System;
using _Project.Scripts._GlobalLogic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.DraggableObjects.Draggable
{
    public abstract class DraggableView : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [field:SerializeField] public RectTransform RectTransform { get; private set; }
        
        public abstract DraggablePresenter BasePresenter { get; protected set; }
        
        private readonly Vector3 _offset = new (0, 200, 0);
        
        public event Action<DraggablePresenter> OnBeginedDrag;
        public event Action<DraggablePresenter> OnEndedDrag;
        
        private void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
            _canvasGroup ??= GetComponent<CanvasGroup>();
        }

        public abstract void Initialize(DraggablePresenter presenter);
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            RectTransform.DOKill();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            RectTransform.DOKill();

            UpdatePosition(eventData);
            OnBeginedDrag?.Invoke(BasePresenter);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData);
        }
        
        private void UpdatePosition(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    RectTransform, eventData.position, GlobalObjects.Camera, out var worldPoint))
            {
                Vector3 worldOffset = RectTransform.TransformVector(_offset);
                var pos = worldPoint + worldOffset;

                var halfWidth = RectTransform.rect.width * 0.5f * RectTransform.lossyScale.x;
                var halfHeight = RectTransform.rect.height * 0.5f * RectTransform.lossyScale.y;

                var min = GlobalObjects.Camera.ScreenToWorldPoint(new Vector3(0, 0, GlobalObjects.Camera.nearClipPlane));
                var max = GlobalObjects.Camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, GlobalObjects.Camera.nearClipPlane));

                pos.x = Mathf.Clamp(pos.x, min.x + halfWidth, max.x - halfWidth);
                pos.y = Mathf.Clamp(pos.y, min.y + halfHeight, max.y - halfHeight);

                RectTransform.position = pos;
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            OnEndedDrag?.Invoke(BasePresenter);
        }

        public virtual Tween Show()
        {
            return _canvasGroup.DOFade(0, 0.5f);
        }
        
        public virtual Tween Hide()
        {
            return _canvasGroup.DOFade(0f, 0.5f);
        }
        
        public virtual void ShowFast()
        {
            _canvasGroup.alpha = 1;
        }
        
        public virtual void HideFast()
        {
            _canvasGroup.alpha = 0;
        }
        
        public virtual void Dispose()
        {
            OnBeginedDrag = null;
            OnEndedDrag = null;
        }
    }
}