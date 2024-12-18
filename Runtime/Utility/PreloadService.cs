using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Networking;
using AdsAppView.DTO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AdsAppView.Utility
{
    [Preserve]
    public class PreloadService
    {
        private const string ControllerName = "AdsApp";
        private const string PayedSettingsRCName = "popup-payed-settings";
        private const string PayedConfigRCName = "popup-payed-configs";
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
        private AppData _appData;
        private int _bundlIdVersion;
        private bool _isEndPrepare = false;
        private bool _freeApp = true;
        private bool _isVip = true;

        public PreloadService(AdsAppAPI api, int bundlIdVersion, bool freeApp, bool vip, AppData appData)
        {
            _api = api;
            _isVip = vip;
            _appData = appData;
            _freeApp = freeApp;
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
            if (_freeApp)
                IsPluginAvailable = await InitFreeApp();
            else
                IsPluginAvailable = await InitPayedApp();

            _isEndPrepare = true;
        }

        private async Task<bool> InitPayedApp()
        {
            string apiName = PayedSettingsRCName;

            RequestPayedPopupData data = new() { app_id = _appData.app_id, platform = _appData.platform, store_id = _appData.store_id, vip = _isVip };
            Response response = await _api.GetAppSettings(ControllerName, apiName, data);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                {
                    Debug.LogError($"#PreloadService# Fail to recieve remote settings '{apiName}': NULL");
                    return false;
                }
                else
                {
                    PopupPayedSettingsData settings = JsonConvert.DeserializeObject<PopupPayedSettingsData>(response.body);

                    Debug.Log($"#PreloadService# Plugin settings: State - {settings.released_state}, release - {settings.released_version}, vip state - {settings.vip_state}\n" +
                        $"---->Review: state - {settings.review_state}, version - {settings.review_version}");

                    if (settings.review_state && _bundlIdVersion == settings.review_version)
                    {
                        return true;
                    }
                    else if (settings.review_state == false && _bundlIdVersion == settings.review_version)
                    {
                        return false;
                    }
                    else if (settings.released_state && _bundlIdVersion <= settings.released_version)
                    {
                        if (_isVip)
                            return settings.vip_state;
                        else
                            return true;
                    }
                    else if (settings.released_state == false && _bundlIdVersion <= settings.released_version)
                    {
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                Debug.LogError($"#PreloadService# Fail to recieve remote settings '{apiName}': " + response.statusCode);
                return false;
            }
        }

        private async Task<bool> InitFreeApp()
        {
            string remoteName = $"{Application.identifier}/{Platform}";
            Response response = await _api.GetRemoteSettings(remoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                {
                    Debug.LogError($"#PreloadService# Fail to recieve remote config '{remoteName}': NULL");
                    return false;
                }
                else
                {
                    PluginSettings remotePluginSettings = JsonConvert.DeserializeObject<PluginSettings>(response.body);

                    Debug.Log($"#PreloadService# Plugin settings: State - {remotePluginSettings.plugin_state}, release - {remotePluginSettings.released_version}\n" +
                        $"---->Test state - {remotePluginSettings.test_review},  review - {remotePluginSettings.review_version}");

                    if (remotePluginSettings.test_review == True && _bundlIdVersion == remotePluginSettings.review_version)
                        return true;
                    else if (remotePluginSettings.test_review != True && _bundlIdVersion == remotePluginSettings.review_version)
                        return false;
                    else if (remotePluginSettings.plugin_state == On && _bundlIdVersion <= remotePluginSettings.released_version)
                        return true;
                    else if (remotePluginSettings.plugin_state != On && _bundlIdVersion <= remotePluginSettings.released_version)
                        return false;
                    else
                        return false;
                }
            }
            else
            {
                Debug.LogError($"#PreloadService# Fail to recieve remote config '{remoteName}': " + response.statusCode);
                return false;
            }
        }
    }
}
