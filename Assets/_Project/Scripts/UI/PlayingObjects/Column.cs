using System.Collections.Generic;
using _Project.Scripts.UI.PlayingObjects.Cell;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects
{
    public class Column : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [field:SerializeField] public List<CellController> Cells { get; private set; }
        public Vector2 FieldSize => _rectTransform.rect.size;

        private void OnValidate()
        {
            _rectTransform ??= GetComponent<RectTransform>();
        }
    }
}