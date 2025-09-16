using _Project.Scripts.Interfaces;
using _Project.Scripts.UI.DraggableObjects.PlayableBlock;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.UI.DraggableObjects.Draggable
{
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(PlayableBlockModel))]
    public abstract partial class DraggableModel : ISavableModel
    {
        public string ParentPath { get; set; }
        public Vector2 SaveAnchoredPosition { get; set; }
        public Quaternion SaveRotation { get; set; }
    }
}