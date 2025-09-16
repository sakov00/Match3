using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.UI.Windows.BaseWindow;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Scripts.SO
{
    [CreateAssetMenu(fileName = "WindowsConfig", menuName = "SO/Windows Config")]
    public class WindowsConfig : ScriptableObject, IInitializable
    {
        [SerializeField] private List<BaseWindowPresenter> windowsList = new();
        
        public Dictionary<Type, BaseWindowPresenter> Windows;
        
        public void Initialize()
        {
            Windows = windowsList.ToDictionary(w => w.GetType(), w => w);
        }
    }
}