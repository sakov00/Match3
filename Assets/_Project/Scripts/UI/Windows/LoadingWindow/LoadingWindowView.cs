using _Project.Scripts.UI.UIEffects;
using _Project.Scripts.UI.Windows.BaseWindow;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI.Windows.LoadingWindow
{
    public class LoadingWindowView : BaseWindowView
    {
        [Header("Presenter")]
        [SerializeField] private LoadingWindowPresenter _presenter;

        [Header("UI Effects")]
        [SerializeField] private RotateUIAroundPoint _rotateUIAroundPoint;

        public override Tween Show()
        {
            var sequenceShow = DOTween.Sequence();
            sequenceShow.AppendCallback(() => _rotateUIAroundPoint.StartRotation());
            sequenceShow.Append(base.Show());
            return sequenceShow;
        }
        
        public override Tween Hide()
        {
            var sequenceHide = DOTween.Sequence();
            sequenceHide.AppendCallback(() => _rotateUIAroundPoint.StopRotation());
            sequenceHide.Append(base.Hide());
            return sequenceHide;
        }
        
        public override void ShowFast()
        {
            base.ShowFast();
            _rotateUIAroundPoint.StartRotation();
        }

        public override void HideFast()
        {
            base.HideFast();
            _rotateUIAroundPoint.StopRotation();
        }
    }
}