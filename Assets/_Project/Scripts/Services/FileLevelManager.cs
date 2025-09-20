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
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.Services
{
    public class FileLevelManager : IInitializable
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private PlayableBlockPool _playableBlockPool;
        
        public int TotalLevels { get; private set; }
        
        private static string GetDefaultSavePath(int index) 
            => Path.Combine(Application.streamingAssetsPath, $"level_{index}.dat");
        private static string GetProgressSavePath(int index) 
            => Path.Combine(Application.persistentDataPath, $"level_progress_{index}.dat");
        

        public void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            TotalLevels = CountLevelsAndroid();
#else
            TotalLevels = Directory.GetFiles(Application.streamingAssetsPath, "level_*.dat").Length;
#endif
            Debug.Log($"Total levels found: {TotalLevels}");
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        private int CountLevelsAndroid()
        {
            var count = 0;
            while (true)
            {
                var path = Path.Combine(Application.streamingAssetsPath, $"level_{count}.dat");
                if (!FileExistsAndroid(path)) break;
                count++;
            }
            return count;
        }

        private bool FileExistsAndroid(string path)
        {
            using var request = UnityWebRequest.Head(path);
            var op = request.SendWebRequest();
            while (!op.isDone) { }
            return request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError;
        }
#endif

        public async UniTask SaveLevelDefault(int index) => await Save(GetDefaultSavePath(index));
        public async UniTask LoadLevelDefault(int index) => await Load(GetDefaultSavePath(index));
        public async UniTask SaveLevelProgress(int index) => await Save(GetProgressSavePath(index));
        public async UniTask LoadLevelProgress(int index) => await Load(GetProgressSavePath(index));

        public async UniTask LoadLevel(int fakeIndex)
        {
            var realIndex = fakeIndex % TotalLevels;

            if (File.Exists(GetProgressSavePath(fakeIndex)))
                await LoadLevelProgress(fakeIndex);
            else
                await LoadLevelDefault(realIndex);
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

        private async UniTask Load(string path)
        {
            byte[] bytes;
            #if UNITY_ANDROID && !UNITY_EDITOR
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
            #else
            if (!File.Exists(path))
            {
                Debug.LogWarning("Save file not found!");
                return;
            }

            bytes = await File.ReadAllBytesAsync(path);
            #endif

            var data = LZ4Pickler.Unpickle(bytes);
            var levelModel = MemoryPackSerializer.Deserialize<LevelModel>(data);
            
            Debug.Log($"Loaded {levelModel.ColumnModels.Count} objects.");
            Debug.Log($"Loaded {levelModel.CellModels.Count} objects.");

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
            }

            await UniTask.WhenAll(tasks);
            await UniTask.Yield();
        }
    }
}
