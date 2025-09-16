using System;
using _Project.Scripts.Enums;
using UnityEngine;

namespace _Project.Scripts.UI.Windows.BaseWindow
{
    [Serializable]
    public class BaseWindowModel
    {
        [SerializeField] protected WindowType _windowType = WindowType.Window;
        
        public virtual WindowType WindowType => _windowType;
    }
}