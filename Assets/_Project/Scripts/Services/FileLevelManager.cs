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

        private static string GetDefaultSavePath(int index) =>
            Path.Combine(Application.streamingAssetsPath, $"level_{index}.dat");

        private static string GetProgressSavePath(int index) =>
            Path.Combine(Application.persistentDataPath, $"level_progress_{index}.dat");

        public async UniTask Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            TotalLevels = await CountLevelsAndroid();
#else
            TotalLevels = CountLevelsPC();
#endif
            Debug.Log($"Total levels found: {TotalLevels}");
        }

        private int CountLevelsPC()
        {
            return Directory.GetFiles(Application.streamingAssetsPath, "level_*.dat").Length;
        }

        private async UniTask<int> CountLevelsAndroid()
        {
            var count = 0;
            while (true)
            {
                try
                {
                    var path = GetDefaultSavePath(count);
                    using var request = UnityWebRequest.Get(path);
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                        break;
                    count++;
                }
                catch (Exception) { break; }
            }
            return count;
        }

        public UniTask SaveLevelDefault(int index) => Save(GetDefaultSavePath(index));
        public UniTask SaveLevelProgress(int index) => Save(GetProgressSavePath(index));

        public UniTask LoadLevelDefault(int index) => LoadFromStreamingAssets(GetDefaultSavePath(index));
        public UniTask LoadLevelProgress(int index) => LoadFromPersistentPath(GetProgressSavePath(index));

        public async UniTask LoadLevel(int fakeIndex)
        {
            if (TotalLevels == 0)
            {
                Debug.LogError("No levels found! Cannot load level.");
                return;
            }

            var realIndex = fakeIndex % TotalLevels;
            var progressPath = GetProgressSavePath(fakeIndex);

            if (File.Exists(progressPath))
                await LoadLevelProgress(fakeIndex);
            else
                await LoadLevelDefault(realIndex);

            Debug.Log($"Loaded level {realIndex}");
        }

        public void RemoveProgress(int fakeIndex)
        {
            var path = GetProgressSavePath(fakeIndex);
            if (File.Exists(path))
                File.Delete(path);
        }

        private async UniTask Save(string path)
        {
            var columns = _objectsRegistry.GetTypedList<ColumnController>();
            var cells = _objectsRegistry.GetTypedList<CellController>();

            var levelModel = new LevelModel
            {
                ColumnModels = columns.Select(c => c.GetSavableModel()).ToList(),
                CellModels = cells.Select(c => c.GetSavableModel()).ToList()
            };

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

            var bytes = await File.ReadAllBytesAsync(path);
            var levelModel = Deserialize(bytes);

            await InstantiateLoadedObjects(levelModel);
        }

        private async UniTask LoadFromStreamingAssets(string path)
        {
            using var request = UnityWebRequest.Get(path);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to load level at {path}: {request.error}");
                return;
            }

            var levelModel = Deserialize(request.downloadHandler.data);
            await InstantiateLoadedObjects(levelModel);
        }

        private static LevelModel Deserialize(byte[] bytes)
        {
            var data = LZ4Pickler.Unpickle(bytes);
            return MemoryPackSerializer.Deserialize<LevelModel>(data);
        }

        private async UniTask InstantiateLoadedObjects(LevelModel? levelModel)
        {
            if (levelModel == null) return;

            var allColumns = _objectsRegistry.GetTypedList<ColumnController>();
            var allCells = _objectsRegistry.GetTypedList<CellController>();
            var tasks = new List<UniTask>();

            foreach (var model in levelModel.ColumnModels)
            {
                var column = allColumns.First(c => c.Model.ColumnIndex == model.ColumnIndex);
                column.SetSavableModel(model);
            }

            foreach (var model in levelModel.CellModels)
            {
                if (model?.PlayableBlockModel == null) continue;

                var cell = allCells.First(c => c.Model.ColumnIndex == model.ColumnIndex && c.Model.RowIndex == model.RowIndex);
                var task = _playableBlockPool
                    .Get<PlayableBlockPresenter>(cell.transform, model.PlayableBlockModel.GroupId)
                    .ContinueWith(blockPresenter =>
                    {
                        cell.SetSavableModel(model);
                        cell.PlayableBlockPresenter = blockPresenter;
                    });

                tasks.Add(task);
                await UniTask.Yield();
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
