using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    public class PlayableBlockView : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [field:SerializeField] public RectTransform RectTransform { get; private set; }
        
        private event Action<PointerEventData> OnBeginedDrag;
        private event Action<PointerEventData> OnEndedDrag;
        
        private void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
            _canvasGroup ??= GetComponent<CanvasGroup>();
        }

        public void Initialize(Action<PointerEventData> onBeginedDrag, Action<PointerEventData> onEndedDrag)
        {
            OnBeginedDrag = onBeginedDrag;
            OnEndedDrag = onEndedDrag;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("OnBeginDrag");
            OnBeginedDrag?.Invoke(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
            OnEndedDrag?.Invoke(eventData);
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