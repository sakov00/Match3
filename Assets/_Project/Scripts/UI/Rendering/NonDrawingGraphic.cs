using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Rendering
{
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(RectTransform))]
    public class NonDrawingGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}