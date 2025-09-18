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
        [SerializeField] private float  _xCurveHeight = 300f;
        
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
            var randomX = NextGaussian(0f, Screen.width * 0.25f); 
            randomX = Mathf.Clamp(randomX, -Screen.width * 0.25f, Screen.width * 0.25f);
            
            _rectTransform.anchoredPosition = new Vector2(randomX, 0);

            _tween = DOVirtual.Float(0f, 1f, randomTime, t =>
                {
                    var x = _xCurve.Evaluate(t) * _xCurveHeight + randomX;
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
        
        private float NextGaussian(float mean = 0f, float stdDev = 1f)
        {
            var u1 = 1.0f - Random.value;
            var u2 = 1.0f - Random.value;
            var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}