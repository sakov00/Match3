using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.UIEffects
{
    public class UITextureSheetAnimator : MonoBehaviour
    {
        [Header("Components")]
        public RawImage rawImage;

        [Header("Sprite Sheet Settings")]
        public int columns = 4;        // Кол-во столбцов в спрайт-листе
        public int rows = 4;           // Кол-во рядов в спрайт-листе
        public int totalFrames = 16;   // Реальное кол-во кадров, чтобы не показывать пустоту

        [Header("Animation Settings")]
        public float framesPerSecond = 12f;
        public bool loop = true;       // Зациклить анимацию

        private int currentFrame;
        private float timer;

        void Update()
        {
            if (totalFrames <= 0) return;

            timer += Time.deltaTime;
            if (timer >= 1f / framesPerSecond)
            {
                timer -= 1f / framesPerSecond;
                currentFrame++;

                if (currentFrame >= totalFrames)
                {
                    if (loop) currentFrame = 0;
                    else currentFrame = totalFrames - 1; // останавливаться на последнем кадре
                }

                int x = currentFrame % columns;
                int y = currentFrame / columns;

                // uvRect для RawImage (от верхнего левого)
                rawImage.uvRect = new Rect(
                    (float)x / columns,
                    1f - (float)(y + 1) / rows,
                    1f / columns,
                    1f / rows
                );
            }
        }

        // Воспроизведение анимации заново
        public void Play()
        {
            currentFrame = 0;
            timer = 0f;
        }

        // Остановить анимацию
        public void Stop()
        {
            enabled = false;
        }
    }
}