using System.Collections.Generic;
using _Project.Scripts.UI.DraggableObjects.PlayableBlock;
using UnityEngine;

namespace _Project.Scripts.SO
{
    [CreateAssetMenu(fileName = "PlayableBlocksConfig", menuName = "SO/Playable Blocks Config")]
    public class PlayableBlocksConfig : ScriptableObject
    {
        [field:SerializeField] public List<PlayableBlockPresenter> ListPlayableBlockPrefabs { get; set; }
    }
}