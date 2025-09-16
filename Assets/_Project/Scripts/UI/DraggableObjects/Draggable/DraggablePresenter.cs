using System;
using _Project.Scripts._GlobalLogic;
using _Project.Scripts.Interfaces;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.DraggableObjects.Draggable
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class DraggablePresenter : MonoBehaviour, IDisposable, ISavableLogic, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public CanvasGroup CanvasGroup { get; set; }
        [field: NonSerialized] public Vector3 RealPos { get; set; }

        private readonly Vector3 _offset = new (0, 200, 0);
        
        private event Action<DraggablePresenter> OnBeginedDrag;
        private event Action<DraggablePresenter> OnEndedDrag;

        private void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
            CanvasGroup ??= GetComponent<CanvasGroup>();
        }

        public virtual void Initialize()
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransform.DOKill();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransform.DOKill();

            UpdatePosition(eventData);
            OnBeginedDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
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

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndedDrag?.Invoke(this);
        }

        public Tween Hide()
        {
            return CanvasGroup.DOFade(0f, 0.5f);
        }
        
        public virtual ISavableModel GetSavableModel()
        {
            throw new NotImplementedException();
        }

        public virtual void SetSavableModel(ISavableModel savableModel)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            OnBeginedDrag = null;
            OnEndedDrag = null;
        }

        private void OnDestroy() => Dispose();

    }
}
