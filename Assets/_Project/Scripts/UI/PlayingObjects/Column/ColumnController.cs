using System;
using System.Collections.Generic;
using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.Cell;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects.Column
{
    public class ColumnController : MonoBehaviour, IDisposable
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        [field:SerializeField] public RectTransform RectTransform { get; private set; }
        [field:SerializeField] public List<CellController> Cells { get; private set; }
        [field:SerializeField] public ColumnModel Model { get; private set; }

        private void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
        }

        private void Awake()
        {
            InjectManager.Inject(this);
            Initialize();
        }

        public void Initialize()
        {
            _objectsRegistry.Register(this);
            Cells.ForEach(x => x.Initialize());
        }
        
        public virtual ColumnModel GetSavableModel()
        {
            return Model;
        }

        public virtual void SetSavableModel(ColumnModel savableModel)
        {
            Model = savableModel;
        }
        
        public void Dispose()
        {   
            Cells.ForEach(x => x.Dispose());
        }
    }
}