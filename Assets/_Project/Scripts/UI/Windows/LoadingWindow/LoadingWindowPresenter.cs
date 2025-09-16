using _Project.Scripts.AllAppData;
using _Project.Scripts.Registries;
using _Project.Scripts.Services;
using _Project.Scripts.UI.Windows.BaseWindow;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.Windows.LoadingWindow
{
    public class LoadingWindowPresenter : BaseWindowPresenter
    {
        [Inject] private AppData _appData;
        [Inject] private ObjectsRegistry _objectsRegistry;
        [Inject] private ResetLevelService _resetLevelService;
        
        [SerializeField] private BaseWindowModel _model;
        [SerializeField] private LoadingWindowView _view;
        
        protected override BaseWindowModel BaseModel => _model;
        protected override BaseWindowView BaseView => _view;
    }
}