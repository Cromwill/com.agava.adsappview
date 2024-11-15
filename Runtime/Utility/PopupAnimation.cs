using Spine.Unity;
using UnityEngine;

namespace AdsAppView.Utility
{
    public class PopupAnimation : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic _skeletonGraphic;

        public void Initialize(AnimationData animationData)
        {
            _skeletonGraphic.skeletonDataAsset = animationData.SkeletonDataAsset;
            _skeletonGraphic.startingLoop = true;
            _skeletonGraphic.startingAnimation = animationData.StartingAnimation;
        }
    }
}
