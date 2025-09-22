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

        private Tween _tweenShow;
        private Tween _tweenHide;
        
        private bool _isShowed;

        public override Tween Show()
        {
            if(_isShowed == true) return _tweenShow;
            
            _isShowed = true;
            _rotateUIAroundPoint.StartRotation();
            _tweenShow = base.Show().OnComplete(() => _tweenShow = null);
            return _tweenShow;
        }
        
        public override Tween Hide()
        {
            if(_isShowed == false) return _tweenHide;
            
            _isShowed = false;
            _rotateUIAroundPoint.StopRotation();
            _tweenHide = base.Hide().OnComplete(() => _tweenHide = null);
            return _tweenHide;
        }
        
        public override void ShowFast()
        {
            _tweenShow?.Complete();
            _isShowed = true;
            base.ShowFast();
            _rotateUIAroundPoint.StartRotation();
        }

        public override void HideFast()
        {
            _tweenHide?.Complete();
            _isShowed = false;
            base.HideFast();
            _rotateUIAroundPoint.StopRotation();
        }
    }
}