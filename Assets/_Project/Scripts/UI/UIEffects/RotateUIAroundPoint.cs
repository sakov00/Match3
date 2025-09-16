using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.UIEffects
{
    public class RotateUIAroundPoint : MonoBehaviour
    {
        [SerializeField] private RectTransform _centerPoint;
        [SerializeField] private RectTransform[] _uiObjects;
        [SerializeField] private float _radius = 50f;
        [SerializeField] private float _duration = 4f;

        private readonly List<Tween> _tweens = new();

        public void StartRotation()
        {
            if (_uiObjects.Length == 0) return;

            for (var i = 0; i < _uiObjects.Length; i++)
            {
                var angle = i * Mathf.PI * 2 / _uiObjects.Length;
                var startPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
                _uiObjects[i].anchoredPosition = _centerPoint.anchoredPosition + startPos;

                _tweens.Add(CreateCircularTween(_uiObjects[i], angle));
            }
        }

        private Tween CreateCircularTween(RectTransform obj, float startAngle)
        {
            var angle = startAngle;
            return DOTween.To(() => angle, x => 
                {
                    angle = x;
                    var pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
                    obj.anchoredPosition = _centerPoint.anchoredPosition + pos;
                }, startAngle + Mathf.PI * 2, _duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        public void StopRotation()
        {
            _tweens.ForEach(tween => tween.Kill());
            _tweens.Clear();
        }

        private void OnDestroy()
        {
            StopRotation();
        }
    }
}