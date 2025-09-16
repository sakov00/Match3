using _Project.Scripts._VContainer;
using _Project.Scripts.Enums;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI.Windows.BaseWindow
{
    public abstract class BaseWindowPresenter : MonoBehaviour
    {
        [Inject] protected WindowsManager WindowsManager { get; private set; }
        
        protected abstract BaseWindowModel BaseModel { get; }
        protected abstract BaseWindowView BaseView { get; }
        public WindowType WindowType => BaseModel.WindowType;

        protected virtual void Awake()
        {
            InjectManager.Inject(this);
        }

        public Tween Show() => BaseView.Show();
        public Tween Hide() => BaseView.Hide();
        public void ShowFast() => BaseView.ShowFast();
        public void HideFast() => BaseView.HideFast();
    }
}