using UnityEngine;

namespace _Project.Scripts._GlobalLogic
{
    public class GlobalObjects : MonoBehaviour
    {
        [SerializeField] private Camera _cameraController;
        private static GlobalObjects Instance { get; set; }
        public static Camera Camera => Instance._cameraController;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}