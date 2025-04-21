using UnityEditor;
using UnityEngine;

namespace AdsAppView.Utility
{
    public class BuildVersionHolder : MonoBehaviour
    {
        [field: SerializeField] public string Version { get; private set; }
        [field: SerializeField] public int BundleId { get; private set; }

        public void ApplyToProjectSettings()
        {
#if UNITY_EDITOR
            PlayerSettings.bundleVersion = Version;
#if UNITY_ANDROID
            PlayerSettings.Android.bundleVersionCode = BundleId;
#elif UNITY_IOS
            PlayerSettings.iOS.buildNumber = BundleId.ToString();
#endif
#endif
        }
    }
}
