using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.UIEffects
{
    [RequireComponent(typeof(RawImage))]
    public class UITextureSheetAnimator : MonoBehaviour
    {
        [Header("Components")]
        public RawImage _rawImage;

        [Header("Animation Settings")]
        public int _columns = 4;
        public int _rows = 4;
        public int _totalFrames = 16;
        public bool _loop = true;

        private int _currentFrame;
        private float _timer;

        private void OnValidate()
        {
            _rawImage ??= GetComponent<RawImage>();
        }

        void Update()
        {
            if (_totalFrames <= 0) return;

            _timer += Time.deltaTime;
            if (_timer >= 1f / _totalFrames)
            {
                _timer -= 1f / _totalFrames;
                _currentFrame++;

                if (_currentFrame >= _totalFrames)
                {
                    if (_loop) _currentFrame = 0;
                    else _currentFrame = _totalFrames - 1;
                }

                var x = _currentFrame % _columns;
                var y = _currentFrame / _columns;

                _rawImage.uvRect = new Rect(
                    (float)x / _columns,
                    1f - (float)(y + 1) / _rows,
                    1f / _columns,
                    1f / _rows
                );
            }
        }

        public void Play()
        {
            _currentFrame = 0;
            _timer = 0f;
        }

        public void Stop()
        {
            enabled = false;
        }
    }
}