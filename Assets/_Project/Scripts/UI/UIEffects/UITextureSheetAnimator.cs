using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.UI.UIEffects
{
    [RequireComponent(typeof(RawImage))]
    public class UITextureSheetAnimator : MonoBehaviour
    {
        [Serializable]
        public class AnimationClipSettings
        {
            public string Name;
            public Texture Texture;
            public int Columns = 4;
            public int Rows = 4;
            public int TotalFrames = 16;
            public bool Loop = false;
            public float FPS = 16f;
        }

        [Header("Components")]
        [SerializeField] private RawImage _rawImage;

        [Header("Animations")]
        public List<AnimationClipSettings> Animations = new();

        private CancellationTokenSource _playCts;

        private void OnValidate()
        {
            _rawImage ??= GetComponent<RawImage>();
        }

        public async UniTask PlayAsync(string animationName, CancellationToken externalToken = default)
        {
            var anim = Animations.Find(a => a.Name == animationName);
            if (anim == null)
            {
                Debug.LogWarning($"Animation '{animationName}' not found!");
                return;
            }

            _playCts?.Cancel();
            _playCts?.Dispose();

            var safeMonoToken = this ? this.GetCancellationTokenOnDestroy() : CancellationToken.None;
            _playCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, safeMonoToken);
            var token = _playCts.Token;

            if (_rawImage == null) return;
            
            _rawImage.texture = anim.Texture;
            var currentFrame = 0;
            var delay = 1f / anim.FPS;

            try
            {
                do
                {
                    for (currentFrame = 0; currentFrame < anim.TotalFrames; currentFrame++)
                    {
                        var x = currentFrame % anim.Columns;
                        var y = currentFrame / anim.Columns;

                        _rawImage.uvRect = new Rect(
                            (float)x / anim.Columns,
                            1f - (float)(y + 1) / anim.Rows,
                            1f / anim.Columns,
                            1f / anim.Rows
                        );

                        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                    }
                }
                while (anim.Loop && !token.IsCancellationRequested);
            }
            catch (OperationCanceledException) { }
        }

        public void Stop()
        {
            _playCts?.Cancel();
            _playCts = null;
        }
    }
}
