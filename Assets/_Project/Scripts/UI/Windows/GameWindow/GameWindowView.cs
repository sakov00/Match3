using _Project.Scripts.AllAppData;
using _Project.Scripts.UI.Windows.BaseWindow;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Scripts.UI.Windows.GameWindow
{
    public class GameWindowView : BaseWindowView
    {
        [Inject] private AppData _appData;
        
        [Header("Presenter")]
        [SerializeField] private GameWindowPresenter _presenter;

        [Header("Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _restartLevelButton;
        
        protected override BaseWindowPresenter BasePresenter => _presenter;
        
        private void Start()
        {
            _presenter.NextLevelCommand.BindTo(_nextLevelButton).AddTo(this);
            _presenter.RestartLevelCommand.BindTo(_restartLevelButton).AddTo(this);
        }
        
        public override Tween Show()
        {
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => gameObject.SetActive(true));
            sequence.Append(_canvasGroup.DOFade(1f, 0.5f).From(0));
            return sequence;
        }

        public override Tween Hide()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(0f, 0.5f).From(1));
            sequence.AppendCallback(() => gameObject.SetActive(false));
            return sequence;
        }
    }
}