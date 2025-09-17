using System;
using _Project.Scripts.UI.DraggableObjects.Draggable;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.UI.DraggableObjects.PlayableBlock
{
    [Serializable]
    [MemoryPackable]
    public partial class PlayableBlockModel : DraggableModel
    {
        [field:SerializeField] public int GroupId { get;set; }
        public int[,] PosOnGameZone { get;set; }
    }
}