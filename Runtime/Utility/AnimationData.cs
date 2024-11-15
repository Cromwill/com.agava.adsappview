using System.Linq;
using Spine.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationData", menuName = "Create new AnimationData", order = 51)]
public class AnimationData : ScriptableObject
{
    [field: SerializeField] public SkeletonDataAsset SkeletonDataAsset { get; private set; }

    public string StartingAnimation => SkeletonDataAsset == null ? string.Empty : SkeletonDataAsset.fromAnimation.First();
}
