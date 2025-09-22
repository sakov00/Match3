using System;
using System.Threading;
using _Project.Scripts.UI.UIEffects;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.UI.PlayingObjects.PlayableBlock
{
    public class PlayableBlockView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private UITextureSheetAnimator _uiTextureSheetAnimator;

        public void Initialize()
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = Vector3.one;
            IdleAnim().Forget();
        }
        
        public virtual async UniTask IdleAnim()
        {
            _uiTextureSheetAnimator.Stop();
            await _uiTextureSheetAnimator.PlayAsync("Idle");
        }
        
        public virtual async UniTask DestroyAnim()
        {
            _uiTextureSheetAnimator.Stop();
            await _uiTextureSheetAnimator.PlayAsync("StartDestroy");
            await _uiTextureSheetAnimator.PlayAsync("EndDestroy");
        }

        public void Dispose()
        {
            _uiTextureSheetAnimator.Stop();
        }
    }
}