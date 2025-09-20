using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts.AllAppData;
using _Project.Scripts.UI.UIEffects;
using _Project.Scripts.UI.Windows.BaseWindow;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Random = UnityEngine.Random;

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
        
        [Header("Balloons")]
        [SerializeField] private List<Balloon> _balloons;
        [SerializeField] private int _maxAnimatedBalloons = 3;
        [SerializeField] private Vector2 _randomDelay = new (3f, 5f);
        
        private CancellationTokenSource _balloonsCts;
        
        private void Start()
        {
            _presenter.NextLevelCommand.BindTo(_nextLevelButton).AddTo(this);
            _presenter.RestartLevelCommand.BindTo(_restartLevelButton).AddTo(this);
        }

        public void Initialize()
        {
            StartBalloonsLoop();
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
        
        private void StartBalloonsLoop()
        {
            StopBalloonsLoop();
            _balloonsCts = new CancellationTokenSource();
            StartBalloons(_balloonsCts.Token).Forget();
        }

        private void StopBalloonsLoop()
        {
            if (_balloonsCts == null) return;
            _balloonsCts.Cancel();
            _balloonsCts.Dispose();
            _balloonsCts = null;
            _balloons.ForEach(balloon => balloon.StopAnimation());
        }
        
        private async UniTaskVoid StartBalloons(CancellationToken token) 
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    
                    var free = _balloons.Where(b => !b.IsAnimating).ToList();
                    var freeBalloon = free.Any() ? free.ElementAt(Random.Range(0, free.Count)) : null;
                    freeBalloon?.StartAnimation();
                    
                    var delay = Random.Range(_randomDelay.x, _randomDelay.y);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            StopBalloonsLoop();
        }
    }
}