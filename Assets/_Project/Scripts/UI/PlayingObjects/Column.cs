using System.Collections.Generic;
using _Project.Scripts.UI.PlayingObjects.Cell;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects
{
    public class Column : MonoBehaviour
    {
        [field:SerializeField] public List<CellController> Cells { get; private set; }
    }
}