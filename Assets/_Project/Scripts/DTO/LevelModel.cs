using System.Collections.Generic;
using _Project.Scripts.UI.PlayingObjects.Column;
using MemoryPack;
using CellModel = _Project.Scripts.UI.PlayingObjects.Cell.CellModel;

namespace _Project.Scripts.DTO
{
    [MemoryPackable]
    public partial class LevelModel
    {
        public List<ColumnModel> ColumnModels { get; set; } = new();
        public List<CellModel> CellModels { get; set; } = new();
    }
}