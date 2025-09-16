using System.IO;
using System.Linq;
using _Project.Scripts.DTO;
using _Project.Scripts.Interfaces;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.DraggableObjects.PlayableBlock;
using Cysharp.Threading.Tasks;
using K4os.Compression.LZ4;
using MemoryPack;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.Services
{
    public class SaveLoadLevelService
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private DraggablePool _draggablePool;
        [Inject] private ResetLevelService _resetLevelService;
        private static string GetDefaultSavePath(int index) 
            => Path.Combine(Application.streamingAssetsPath, $"level_{index}.dat");
        private static string GetProgressSavePath(int index) 
            => Path.Combine(Application.persistentDataPath, $"level_progress_{index}.dat");

        public async UniTask SaveLevelDefault(int index) => await Save(GetDefaultSavePath(index));
        public async UniTask LoadLevelDefault(int index) => await Load(GetDefaultSavePath(index));
        public async UniTask SaveLevelProgress(int index) => await Save(GetProgressSavePath(index));
        public async UniTask LoadLevelProgress(int index) => await Load(GetProgressSavePath(index));

        public async UniTask LoadLevel(int index)
        {
            if (File.Exists(GetProgressSavePath(index)))
                await LoadLevelDefault(index);
            else
                await LoadLevelProgress(index);
        }

        public void RemoveProgress(int index)
        {
            if(File.Exists(GetProgressSavePath(index)))
                File.Delete(GetProgressSavePath(index));
        }

        private async UniTask Save(string path)
        {
            var allObjects = _objectsRegistry.GetAllByInterface<ISavableLogic>();
            
            var levelModel = new LevelModel();
            levelModel.SavableModels.AddRange(allObjects.Select(o => o.GetSavableModel()).ToList());

            var data = MemoryPackSerializer.Serialize(levelModel);
            var compressed = LZ4Pickler.Pickle(data);
            await File.WriteAllBytesAsync(path, compressed);
            
            Debug.Log($"Level saved to {path}");
        }

        private async UniTask Load(string path)
        {
            _resetLevelService.ResetLevel();
            if (!File.Exists(path))
            {
                Debug.LogWarning("Save file not found!");
                return;
            }

            var compressed = await File.ReadAllBytesAsync(path);
            var data = LZ4Pickler.Unpickle(compressed);
            LevelModel levelModel = MemoryPackSerializer.Deserialize<LevelModel>(data);

            Debug.Log($"Loaded {levelModel.SavableModels.Count} objects.");

            await InstantiateLoadedObjects(levelModel);
        }

        private async UniTask InstantiateLoadedObjects(LevelModel levelModel)
        {
            foreach (var model in levelModel.SavableModels)
            {
                var parent = GameObject.Find(model.ParentPath)?.transform;
                if (parent == null) return;
                
                ISavableLogic savableLogic = model switch
                {
                    PlayableBlockModel => 
                        _draggablePool.Get<PlayableBlockPresenter>(parent, model.SaveAnchoredPosition, model.SaveRotation),
                    _ => null
                };

                savableLogic?.SetSavableModel(model);

                await UniTask.Yield();
            }
        }
    }
}
