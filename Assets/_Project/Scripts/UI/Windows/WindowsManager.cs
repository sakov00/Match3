using System;
using System.Collections.Generic;
using _Project.Scripts._VContainer;
using _Project.Scripts.Enums;
using _Project.Scripts.SO;
using _Project.Scripts.UI.Windows.BaseWindow;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.UI.Windows
{
    public class WindowsManager : MonoBehaviour, IInitializable
    {
        [Header("Layers")]
        [SerializeField] private Transform _windowsLayer;
        [SerializeField] private Transform _popupLayer;
        [SerializeField] private Transform _otherLayer;
        
        [Header("Layers")]
        [SerializeField] private Image _darkBackground;
        
        [Inject] private IObjectResolver _resolver;
        [Inject] private WindowsConfig _windowsConfig;
        
        private readonly Dictionary<Type, BaseWindowPresenter> _cachedWindows = new ();
        
        public void Initialize()
        {
            InjectManager.Inject(this);
        }
        
        public T GetWindow<T>() where T : BaseWindowPresenter
        {
            var windowType = _windowsConfig.Windows[typeof(T)].WindowType;
            if (!_cachedWindows.TryGetValue(typeof(T), out var window))
            {
                window = _resolver.Instantiate(_windowsConfig.Windows[typeof(T)], parent: GetParent(windowType));
                _cachedWindows.Add(typeof(T), window);
            }
            return window as T;
        }
        
        public Tween ShowWindow<T>() where T : BaseWindowPresenter
        {
            var windowType = _windowsConfig.Windows[typeof(T)].WindowType;
            if (!_cachedWindows.TryGetValue(typeof(T), out var window))
            {
                window = _resolver.Instantiate(_windowsConfig.Windows[typeof(T)], parent: GetParent(windowType));
                _cachedWindows.Add(typeof(T), window);
            }
            
            var sequence = DOTween.Sequence();
            sequence.Append(window.Show());
            if (windowType == WindowType.Popup) sequence.Join(ShowDarkBackground());
            sequence.SetUpdate(true);
            return sequence;
        }
        
        public Tween HideWindow<T>() where T : BaseWindowPresenter
        {
            var windowType = _windowsConfig.Windows[typeof(T)].WindowType;
            if (!_cachedWindows.TryGetValue(typeof(T), out var window))
            {
                window = _resolver.Instantiate(_windowsConfig.Windows[typeof(T)], parent: GetParent(windowType));
                _cachedWindows.Add(typeof(T), window);
            }
    
            var sequence = DOTween.Sequence();
            sequence.Append(window.Hide());
            if (windowType == WindowType.Popup) sequence.Join(HideDarkBackground());
            sequence.SetUpdate(true);
            return sequence;
        }
        
        private Tween ShowDarkBackground()
        {
            var color = _darkBackground.color;
            color.a = 0.5f;
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => _darkBackground.gameObject.SetActive(true));
            sequence.Append(_darkBackground.DOColor(color, 0.5f));
            return sequence;
        }
        
        private Tween HideDarkBackground()
        {
            var color = _darkBackground.color;
            color.a = 0f;
            var sequence = DOTween.Sequence();
            sequence.Append(_darkBackground.DOColor(color, 0.5f));
            sequence.AppendCallback(() => _darkBackground.gameObject.SetActive(false));
            return sequence;
        }
        
        private Transform GetParent(WindowType type)
        {
            return type switch
            {
                WindowType.Window => _windowsLayer,
                WindowType.Popup => _popupLayer,
                WindowType.Other => _otherLayer,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
    }
}