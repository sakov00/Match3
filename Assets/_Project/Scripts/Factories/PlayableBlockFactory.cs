using _Project.Scripts.SO;
using _Project.Scripts.UI.DraggableObjects.Draggable;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.Factories
{
    public class PlayableBlockFactory
    {
        [Inject] private IObjectResolver _resolver;
        [Inject] private PlayableBlocksConfig _playableBlocksConfig;
        
        public T CreatePlayableBlock<T>(Transform parent, int groupId, Vector2 anchoredPos = default, Quaternion rotation = default) where T : DraggablePresenter
        {
            DraggablePresenter draggablePresenter = null;
            foreach (var prefab in _playableBlocksConfig.ListPlayableBlockPrefabs)
            {
                if (prefab is T tPrefab && prefab.Model.GroupId == groupId)
                {
                    draggablePresenter = _resolver.Instantiate(tPrefab, Vector3.zero, rotation, parent);
                    draggablePresenter.BaseView.RectTransform.anchoredPosition = anchoredPos;
                    draggablePresenter.Initialize();
                    break;
                }
            }

            return (T)draggablePresenter;
        }
    }
}