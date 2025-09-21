using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Project.Scripts.DTO;
using _Project.Scripts.Pools;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using _Project.Scripts.UI.PlayingObjects.Column;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using Cysharp.Threading.Tasks;
using K4os.Compression.LZ4;
using MemoryPack;
using UnityEngine;
using UnityEngine.Networking;
using VContainer;

namespace _Project.Scripts.Services
{
    public class FileLevelManager
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private PlayableBlockPool _playableBlockPool;
        
        public int TotalLevels { get; private set; }
        
        private static string GetDefaultSavePath(int index) 
            => Path.Combine(Application.streamingAssetsPath, $"level_{index}.dat");
        private static string GetProgressSavePath(int index) 
            => Path.Combine(Application.persistentDataPath, $"level_progress_{index}.dat");
        
        public async UniTask Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("CountLevelsAndroid");
            TotalLevels = await CountLevelsAndroid();
#else
            Debug.Log("CountLevelsPC");
            TotalLevels = CountLevelsPC();
#endif
            Debug.Log($"Total levels found: {TotalLevels}");
        }

        private int CountLevelsPC()
        {
            var files = Directory.GetFiles(Application.streamingAssetsPath, "level_*.dat");
            return files.Length;
        }

        private async UniTask<int> CountLevelsAndroid()
        {
            int count = 0;

            while (true)
            {
                try
                {
                    var path = Path.Combine(Application.streamingAssetsPath, $"level_{count}.dat");
                    Debug.Log(path);
                    using var request = UnityWebRequest.Get(path);
                    await request.SendWebRequest();
                    
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        break;
                    }
                    
                    count++;
                }
                catch (Exception)
                {
                    break;
                }
            }

            return count;
        }

        public async UniTask SaveLevelDefault(int index) => await Save(GetDefaultSavePath(index));
        public async UniTask LoadLevelDefault(int index) => await LoadFromStreamingAssets(GetDefaultSavePath(index));
        public async UniTask SaveLevelProgress(int index) => await Save(GetProgressSavePath(index));
        public async UniTask LoadLevelProgress(int index) => await LoadFromPersistentPath(GetProgressSavePath(index));

        public async UniTask LoadLevel(int fakeIndex)
        {
            if (TotalLevels == 0)
            {
                Debug.LogError("No levels found! Cannot load level.");
                return;
            }
            
            var realIndex = fakeIndex % TotalLevels;

            if (File.Exists(GetProgressSavePath(fakeIndex)))
                await LoadLevelProgress(fakeIndex);
            else
                await LoadLevelDefault(realIndex);
            
            Debug.Log($"Loaded level {realIndex}");
        } 

        public void RemoveProgress(int fakeIndex)
        {
            if (File.Exists(GetProgressSavePath(fakeIndex)))
                File.Delete(GetProgressSavePath(fakeIndex));
        }

        private async UniTask Save(string path)
        {
            var allColumns = _objectsRegistry.GetTypedList<ColumnController>();
            var allCells = _objectsRegistry.GetTypedList<CellController>();
            var levelModel = new LevelModel();
            levelModel.ColumnModels.AddRange(allColumns.Select(o => o.GetSavableModel()).ToList());
            levelModel.CellModels.AddRange(allCells.Select(o => o.GetSavableModel()).ToList());

            var data = MemoryPackSerializer.Serialize(levelModel);
            var compressed = LZ4Pickler.Pickle(data);
            await File.WriteAllBytesAsync(path, compressed);

            Debug.Log($"Level saved to {path}");
        }

        private async UniTask LoadFromPersistentPath(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("Save file not found!");
                return;
            }

            byte[] bytes = await File.ReadAllBytesAsync(path);
       

            var data = LZ4Pickler.Unpickle(bytes);
            var levelModel = MemoryPackSerializer.Deserialize<LevelModel>(data);

            await InstantiateLoadedObjects(levelModel);
        }
        
        private async UniTask LoadFromStreamingAssets(string path)
        {
            byte[] bytes;
            using (var request = UnityWebRequest.Get(path))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to load level at {path}: {request.error}");
                    return;
                }
                bytes = request.downloadHandler.data;
            }

            var data = LZ4Pickler.Unpickle(bytes);
            var levelModel = MemoryPackSerializer.Deserialize<LevelModel>(data);
            

            await InstantiateLoadedObjects(levelModel);
        }

        private async UniTask InstantiateLoadedObjects(LevelModel levelModel)
        {
            var allColumns = _objectsRegistry.GetTypedList<ColumnController>();
            var allCells = _objectsRegistry.GetTypedList<CellController>();
            var tasks = new List<UniTask>();
            
            foreach (var model in levelModel.ColumnModels)
            {
                var needColumn = allColumns.First(x => x.Model.ColumnIndex == model.ColumnIndex);
                needColumn.SetSavableModel(model);
            }

            foreach (var model in levelModel.CellModels)
            {
                if (model?.PlayableBlockModel == null) continue;

                var needCell = allCells.First(x => x.Model.ColumnIndex == model.ColumnIndex && x.Model.RowIndex == model.RowIndex);

                var task = _playableBlockPool.Get<PlayableBlockPresenter>(needCell.transform, model.PlayableBlockModel.GroupId)
                    .ContinueWith(blockPresenter =>
                    {
                        needCell.SetSavableModel(model);
                        needCell.PlayableBlockPresenter = blockPresenter;
                    });

                tasks.Add(task);
                await UniTask.Yield();
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
