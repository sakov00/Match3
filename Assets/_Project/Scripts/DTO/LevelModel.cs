using System.Collections.Generic;
using MemoryPack;
using CellModel = _Project.Scripts.UI.PlayingObjects.Cell.CellModel;

namespace _Project.Scripts.DTO
{
    [MemoryPackable]
    public partial class LevelModel
    {
        public List<CellModel> SavableModels { get; set; } = new();
    }
}