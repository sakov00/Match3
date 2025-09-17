using UnityEngine;

namespace _Project.Scripts.Extensions
{
    public static class RectTransformExtensions
    {
        public static bool IsOverlappingAnchored(this RectTransform a, RectTransform b,
            float offsetX = 0, float offsetY = 0, float extraRadius = 0f)
        {
            var commonParent = a.root as RectTransform;
            var rectA = GetLocalRectInParent(a, commonParent, extraRadius);
            var rectB = GetLocalRectInParent(b, commonParent, offsetX, offsetY, extraRadius);
            return rectA.Overlaps(rectB);
        }

        public static Rect GetLocalRectInParent(RectTransform rectTransform, RectTransform parent,
            float offsetX = 0, float offsetY = 0, float extraRadius = 0)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            for (int i = 0; i < 4; i++)
                corners[i] = parent.InverseTransformPoint(corners[i]);

            var bottomLeft = corners[0];
            var topRight   = corners[2];

            var rect = new Rect(bottomLeft, topRight - bottomLeft);

            rect.x += offsetX;
            rect.y += offsetY;

            rect.xMin -= extraRadius;
            rect.yMin -= extraRadius;
            rect.xMax += extraRadius;
            rect.yMax += extraRadius;

            return rect;
        }
        
        public static string GetFullPath(this Transform transform)
        {
            if (transform == null) return null;
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}