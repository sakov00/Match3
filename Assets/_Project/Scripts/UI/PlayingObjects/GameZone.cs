using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects
{
    public class GameZone : MonoBehaviour
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        [SerializeField] private List<Column> _columns;
        
        private readonly List<PlayableBlockPresenter> _allBlocks = new();
        
        private Vector2 _dragStartPos;
        private PlayableBlockPresenter _draggedBlock;
        
        private void Awake()
        {
            InjectManager.Inject(this);
        }

        public void Initialize()
        {
            _allBlocks.Clear();
            _columns.ForEach(x => x.Cells.ForEach(y=>
            {
                if (y.PlayableBlockPresenter == null) return;
                y.PlayableBlockPresenter.OnBeginedDrag += OnBeginDrag;
                y.PlayableBlockPresenter.OnEndedDrag += OnEndedDrag;
                _allBlocks.Add(y.PlayableBlockPresenter);
            }));
        }

        private void OnBeginDrag(PointerEventData eventData, PlayableBlockPresenter blockPresenter)
        {
            _draggedBlock = blockPresenter;
            _dragStartPos = eventData.position;
        }

        private void OnEndedDrag(PointerEventData eventData, PlayableBlockPresenter blockPresenter)
        {
            var dragEndPos = eventData.position;
            var delta = dragEndPos - _dragStartPos;

            if (delta.magnitude < 20f)
                return;

            Vector2Int direction;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

            // MoveBlock(blockPresenter, direction);
    
            _draggedBlock = null;
        }
        public void Dispose()
        {
            foreach (var block in _allBlocks)
            {
                block.OnBeginedDrag -= OnBeginDrag;
                block.OnEndedDrag -= OnEndedDrag;
            }
        }
    }
}