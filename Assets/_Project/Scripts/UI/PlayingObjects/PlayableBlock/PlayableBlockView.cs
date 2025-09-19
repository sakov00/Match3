using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    public class PlayableBlockView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private void OnValidate()
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
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
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 1;
        }
        
        public virtual void HideFast()
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0;
        }
    }
}