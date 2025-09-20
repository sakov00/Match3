using _Project.Scripts.Enums;
using _Project.Scripts.UI.Windows.BaseWindow;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.Windows.LoadingWindow
{
    public class LoadingWindowPresenter : BaseWindowPresenter
    {
        [SerializeField] private BaseWindowModel _model;
        [SerializeField] private LoadingWindowView _view;
        public override WindowType WindowType => _model.WindowType;
        public override Tween Show() => _view.Show();
        public override Tween Hide() => _view.Hide();
        public override void ShowFast() => _view.ShowFast();
        public override void HideFast() => _view.HideFast();
    }
}