using System;
using MemoryPack;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    [Serializable]
    [MemoryPackable]
    public partial class PlayableBlockModel
    {
        [field:SerializeField] public int GroupId { get;set; }
    }
}