using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Networking;
using AdsAppView.DTO;
using Newtonsoft.Json;

namespace AdsAppView.Utility
{
    [Preserve]
    public class PreloadService
    {
        private const string True = "true";
        private const string On = "on";
#if UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "android";
#elif UNITY_IOS
        private const string Platform = "ios";
#endif

        private AdsAppAPI _api;
        private int _bundlIdVersion;
        private bool _isEndPrepare = false;

        public PreloadService(AdsAppAPI api, int bundlIdVersion)
        {
            _api = api;
            _bundlIdVersion = bundlIdVersion;
        }

        public bool IsPluginAvailable { get; private set; } = false;

        public IEnumerator Preparing()
        {
            yield return new WaitUntil(() => _api.Initialized);
            yield return null;

            SetPluginAwailable();
            yield return new WaitUntil(() => _isEndPrepare);

            Debug.Log("#PreloadService# Prepare is done. Start plugin " + IsPluginAvailable);
        }

        private async void SetPluginAwailable()
        {
            string remoteName = $"{Application.identifier}/{Platform}";
            var response = await _api.GetPluginSettings(remoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                {
                    IsPluginAvailable = false;
                    Debug.LogError($"#PreloadService# Fail to recieve remote config '{remoteName}': NULL");
                }
                else
                {
                    PluginSettings remotePluginSettings = JsonConvert.DeserializeObject<PluginSettings>(response.body);

                    Debug.Log($"#PreloadService# Plugin settings: State - {remotePluginSettings.plugin_state}, release - {remotePluginSettings.released_version}\n" +
                        $"---->Test state - {remotePluginSettings.test_review},  review - {remotePluginSettings.review_version}");

                    if (remotePluginSettings.test_review == True && _bundlIdVersion == remotePluginSettings.review_version)
                        IsPluginAvailable = true;
                    else if (remotePluginSettings.test_review != True && _bundlIdVersion == remotePluginSettings.review_version)
                        IsPluginAvailable = false;
                    else if (remotePluginSettings.plugin_state == On && _bundlIdVersion <= remotePluginSettings.released_version)
                        IsPluginAvailable = true;
                    else if (remotePluginSettings.plugin_state != On && _bundlIdVersion <= remotePluginSettings.released_version)
                        IsPluginAvailable = false;
                    else
                        IsPluginAvailable = false;
                }
            }
            else
            {
                IsPluginAvailable = false;
                Debug.LogError($"#PreloadService# Fail to recieve remote config '{remoteName}': " + response.statusCode);
            }

            _isEndPrepare = true;
        }
    }
}
