using System.Collections.Generic;
using _Project.Scripts.Interfaces;
using MemoryPack;

namespace _Project.Scripts.DTO
{
    [MemoryPackable]
    public partial class LevelModel
    {
        public List<ISavableModel> SavableModels { get; set; } = new();
    }
}