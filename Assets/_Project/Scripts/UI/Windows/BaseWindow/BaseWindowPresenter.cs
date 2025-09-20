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
        public abstract WindowType WindowType { get; }

        protected virtual void Awake()
        {
            InjectManager.Inject(this);
        }

        public abstract Tween Show();
        public abstract Tween Hide();
        public abstract void ShowFast();
        public abstract void HideFast();
    }
}