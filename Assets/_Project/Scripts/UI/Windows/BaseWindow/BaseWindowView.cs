using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.Windows.BaseWindow
{
    public abstract class BaseWindowView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        
        public virtual Tween Show()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => gameObject.SetActive(true));
            sequence.Append(_canvasGroup.DOFade(1f, 0.5f).From(0));
            return sequence;
        }

        public virtual Tween Hide()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(0f, 0.5f).From(1));
            sequence.AppendCallback(() => gameObject.SetActive(false));
            return sequence;
        }
        
        public virtual void ShowFast()
        {
            _canvasGroup.alpha = 1f;
        }

        public virtual void HideFast()
        {
            _canvasGroup.alpha = 0;
        }
    }
}