using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts._VContainer
{
    public class InjectManager
    {
        private static IObjectResolver _objectResolver;
        
        public static void Initialize(IObjectResolver objectResolverParam)
        {
            _objectResolver = objectResolverParam;
        }

        public static void Inject(object target)
        {
            if (_objectResolver == null)
            {
                Debug.LogError("InjectManager is not initialized.");
                return;
            }

            _objectResolver.Inject(target);
        }

        public static void InjectGameObject(GameObject gameObject)
        {
            if (_objectResolver == null)
            {
                Debug.LogError("InjectManager is not initialized.");
                return;
            }

            _objectResolver.InjectGameObject(gameObject);
        }
    }
}