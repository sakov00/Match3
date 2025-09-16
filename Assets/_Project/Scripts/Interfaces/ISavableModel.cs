using _Project.Scripts.UI.DraggableObjects.Draggable;
using _Project.Scripts.UI.DraggableObjects.PlayableBlock;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.Interfaces
{
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(DraggableModel))]
    [MemoryPackUnion(1, typeof(PlayableBlockModel))]
    public partial interface ISavableModel
    {
        public string ParentPath { get; set; }
        public Vector2 SaveAnchoredPosition { get; set; }
        public Quaternion SaveRotation { get; set; }
    }
}