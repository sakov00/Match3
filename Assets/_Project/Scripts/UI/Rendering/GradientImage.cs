using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Rendering
{
    [RequireComponent(typeof(Graphic))]
    public class GradientImage : BaseMeshEffect
    {
        [SerializeField] private Color _topColor = Color.white;
        [SerializeField] private Color _bottomColor = Color.black;

        public Color TopColor
        {
            get => _topColor;
            set { _topColor = value; Refresh(); }
        }

        public Color BottomColor
        {
            get => _bottomColor;
            set { _bottomColor = value; Refresh(); }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Refresh();
        }

        private void Refresh()
        {
            graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;

            var rect = ((RectTransform)transform).rect;
            UIVertex vert = new UIVertex();
            int count = vh.currentVertCount;

            for (int i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                float lerp = Mathf.InverseLerp(rect.yMin, rect.yMax, vert.position.y);
                vert.color = Color.Lerp(_bottomColor, _topColor, lerp);
                vh.SetUIVertex(vert, i);
            }
        }
    }
}