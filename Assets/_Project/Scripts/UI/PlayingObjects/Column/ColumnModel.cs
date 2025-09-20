using System;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.Column
{
    [Serializable]
    [MemoryPackable]
    public partial class ColumnModel
    {
        [field:SerializeField] public int ColumnIndex { get; set; }
        [field:SerializeField] public bool ColumnIsActive { get; set; }
    }
}