using System.Collections;
using System.Threading.Tasks;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace AdsAppView.Program
{
    public class ViewPresenterConfigs : MonoBehaviour
    {
        public const string DefaultViewPresenterType = null;
        public const float DefaultClosingDelay = 2;
        public const float DefaultEnablingTime = 2;

        private const string ConfigViewPresenterType = "view-presenter-type";
        private const string ConfigClosingDelay = "closing-delay";
        private const string ConfigEnablingTime = "enabling-time";

        private AdsAppAPI _api;
        private bool _initialized;

        public static string ViewPresenterType { get; private set; } = DefaultViewPresenterType;
        public static float ClosingDelay { get; private set; } = DefaultClosingDelay;
        public static float EnablingTime { get; private set; } = DefaultEnablingTime;

        public IEnumerator Initialize(AdsAppAPI api)
        {
            _api = api;

            var waitWeb = new WaitUntil(() => Application.internetReachability == NetworkReachability.NotReachable);
            var waitInit = new WaitUntil(() => _api.Initialized);

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return waitWeb;

            yield return waitInit;
            yield return new WaitForSecondsRealtime(3f);

            _initialized = false;
            SetConfigs();

            yield return new WaitUntil(() => _initialized);
        }

        private async void SetConfigs()
        {
            await SetViewPresenterTypeConfig();
            await SetClosingDelayConfig();
            await SetEnablingTimeConfig();

            _initialized = true;
        }

        private async Task SetClosingDelayConfig()
        {
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(ConfigClosingDelay);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (float.TryParse(body, out float closingDelay))
                {
                    ClosingDelay = closingDelay;
#if UNITY_EDITOR
                    Debug.Log("#ViewPresenterConfigs# Closing delay set to: " + ClosingDelay);
#endif
                }
            }
            else
            {
                Debug.LogError("#ViewPresenterConfigs# Fail to Set Closing Delay Config with error: " + cachingResponse.statusCode);
            }
        }

        private async Task SetEnablingTimeConfig()
        {
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(ConfigEnablingTime);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (float.TryParse(body, out float enablingTime))
                {
                    EnablingTime = enablingTime;
#if UNITY_EDITOR
                    Debug.Log("#ViewPresenterConfigs# Enabling time set to: " + EnablingTime);
#endif
                }
            }
            else
            {
                Debug.LogError("#ViewPresenterConfigs# Fail to Set Enabling Time Config with error: " + cachingResponse.statusCode);
            }
        }

        private async Task SetViewPresenterTypeConfig()
        {
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(ConfigViewPresenterType);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string type = cachingResponse.body;

                if (string.IsNullOrEmpty(type) == false)
                {
                    ViewPresenterType = type;
#if UNITY_EDITOR
                    Debug.Log("#ViewPresenterConfigs# View presenter type set to: " + ViewPresenterType);
#endif
                }
            }
            else
            {
                Debug.LogError("#ViewPresenterConfigs# Fail to Set view presenter type with error: " + cachingResponse.statusCode);
            }
        }
    }
}
