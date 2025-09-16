using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.UIEffects
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class AnimatedUI : MaskableGraphic
    {
        [Header("SpriteSheet")]
        public Texture2D spriteSheet;
        public int columns = 4;
        public int rows = 4;

        [Header("Animation")]
        public int totalFrames = 16;
        public float fps = 10f;
        public bool loop = true;

        private Material _materialInstance;
        private int currentFrame = 0;
        private float timer = 0f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            vh.AddVert(new Vector3(0, 0), color, new Vector2(0, 0));
            vh.AddVert(new Vector3(1, 0), color, new Vector2(1, 0));
            vh.AddVert(new Vector3(1, 1), color, new Vector2(1, 1));
            vh.AddVert(new Vector3(0, 1), color, new Vector2(0, 1));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Создаём инстанцию материала
            if (material != null)
                _materialInstance = new Material(material);
            this.material = _materialInstance;

            // Обновляем свойства материала
            UpdateMaterial();
        }

        private void Update()
        {
            if (totalFrames <= 1 || fps <= 0) return;

            timer += Time.deltaTime;
            if (timer >= 1f / fps)
            {
                timer -= 1f / fps;
                currentFrame++;
                if (currentFrame >= totalFrames)
                {
                    if (loop) currentFrame = 0;
                    else currentFrame = totalFrames - 1;
                }
                UpdateMaterial();
                SetVerticesDirty();
            }
        }

        private void UpdateMaterial()
        {
            if (_materialInstance != null)
            {
                _materialInstance.SetInt("_FrameIndex", currentFrame);
                _materialInstance.SetInt("_Columns", columns);
                _materialInstance.SetInt("_Rows", rows);
            }
        }
    }
}