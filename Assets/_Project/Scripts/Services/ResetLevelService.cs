using System.Linq;
using _Project.Scripts.AllAppData;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using VContainer;

namespace _Project.Scripts.Services
{
    public class ResetLevelService
    {
        [Inject] private AppData _appData;
        [Inject] private PlayableBlockPool _playableBlockPool;
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        public void ResetLevel()
        {
            var cellsList = _objectsRegistry.GetTypedList<CellController>();
            foreach (var cell in cellsList)
                cell.Dispose();
            
            var playableBlocks = _objectsRegistry.GetTypedList<PlayableBlockPresenter>().ToList();
            foreach (var obj in playableBlocks)
                obj.ReturnToPool();
            playableBlocks.Clear();
        }
    }
}