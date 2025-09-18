using System;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.Cell
{
    [Serializable]
    [MemoryPackable]
    public partial class CellModel
    {
        [field:SerializeField] public int RowIndex { get; private set; }
        [field:SerializeField] public int ColumnIndex { get; private set; }
        public PlayableBlockModel PlayableBlockModel { get; set; }
    }
}