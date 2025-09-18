using _Project.Scripts._VContainer;
using _Project.Scripts.Registries;
using _Project.Scripts.UI.PlayingObjects.PlayableBlock;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.PlayingObjects.Cell
{
    public class CellController : MonoBehaviour
    {
        [Inject] private ObjectsRegistry _objectsRegistry;
        
        [field:SerializeField] public CellModel Model { get; private set; }
        [field:SerializeField] public PlayableBlockPresenter PlayableBlockPresenter { get; set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PlayableBlockPresenter == null)
            {
                PlayableBlockPresenter = GetComponentInChildren<PlayableBlockPresenter>();
                if (PlayableBlockPresenter != null)
                {
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif

        private void Awake()
        {
            InjectManager.Inject(this);
            _objectsRegistry.Register(this);
        }
        
        public virtual CellModel GetSavableModel()
        {
            if (PlayableBlockPresenter == null) 
                return null;
            
            Model.PlayableBlockModel = PlayableBlockPresenter.Model;
            return Model;
        }

        public virtual void SetSavableModel(CellModel savableModel)
        {
            Model = savableModel;
        }

        public void Dispose()
        {
            PlayableBlockPresenter = null;
        }
    }
}