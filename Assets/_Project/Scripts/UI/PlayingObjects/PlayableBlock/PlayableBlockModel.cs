using System;
using _Project.Scripts.Enums;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    [Serializable]
    [MemoryPackable]
    public partial class PlayableBlockModel
    {
        [field:SerializeField] public BlockState State { get; set; } = BlockState.Idle;
        [field:SerializeField] public int GroupId { get;set; }
    }
}