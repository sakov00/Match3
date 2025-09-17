using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.UIEffects
{
    public class Balloon : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        
        [Header("Randoms")]
        [SerializeField] private Vector2 _randomScale = new (0.8f, 1.5f);
        [SerializeField] private Vector2 _randomTime = new (7f, 10f);
        
        [Header("Curves")]
        [SerializeField] private AnimationCurve _xCurve;
        [SerializeField] private AnimationCurve _yCurve;
        
        private Tween _tween;
        private Vector3 _defaultSizeDelta;
        
        public bool IsAnimating { get; private set; }

        private void OnValidate()
        {
            _rectTransform ??= GetComponent<RectTransform>();
        }

        private void Start()
        {
            _defaultSizeDelta = _rectTransform.sizeDelta;
        }

        public void StartAnimation()
        {
            IsAnimating = true;
            _rectTransform.sizeDelta = _defaultSizeDelta * Random.Range(_randomScale.x, _randomScale.y);
            var randomTime = Random.Range(_randomTime.x, _randomTime.y);
            var randomX = Random.Range(
                -Screen.width / 2f + _rectTransform.rect.width / 2 + 150f,
                Screen.width / 2f - _rectTransform.rect.width / 2 - 150f);
            
            _rectTransform.anchoredPosition = new Vector2(randomX, 0);

            _tween = DOVirtual.Float(0f, -1f, randomTime, t =>
                {
                    var x = _xCurve.Evaluate(t) * 300 + randomX;
                    var y = _yCurve.Evaluate(t) * (Screen.height + _rectTransform.rect.height);
                    _rectTransform.anchoredPosition = new Vector2(x, y);
                })
                .SetEase(Ease.Linear)
                .OnComplete(() => IsAnimating = false);
        }

        public void StopAnimation()
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            IsAnimating = false;
            _tween?.Kill();
        }
    }
}