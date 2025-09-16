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
        
        protected override BaseWindowPresenter BasePresenter => _presenter;

        public override Tween Show()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => _rotateUIAroundPoint.StartRotation());
            sequence.Append(base.Show());
            return sequence;
        }
        
        public override Tween Hide()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => _rotateUIAroundPoint.StopRotation());
            sequence.Append(base.Hide());
            return sequence;
        }
    }
}