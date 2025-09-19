using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.PlayingObjects.GameZoneLogic
{
    public static class DragService
    {
        public static Vector2 GetDelta(PointerEventData eventData, Vector2 dragStartPos)
        {
            return eventData.position - dragStartPos;
        }

        public static Vector2Int GetDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}