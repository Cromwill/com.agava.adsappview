using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using AdsAppView.Utility;

namespace AdsAppView.Program
{
    public class Links : MonoBehaviour
    {
        private const string SupportRmtKey = "support_bot";
        private const string AgreementRmtKey = "agreement";
        private const string PrivacyRmtKey = "privacy";
        private const string SubscriptionRmtKey = "subscription";

        private AdsAppAPI _api;

        public static string Support { get; private set; } = "https://t.me/MTgames_support_bot";
        public static string Agreement { get; private set; } = "https://mt.media/agreement/";
        public static string Privacy { get; private set; } = "https://mt.media/privacy/";
        public static string Subscription { get; private set; } = "https://wink.ru/services/winkkids";

        public IEnumerator Initialize(AdsAppAPI api)
        {
            _api = api;
            var waitWeb = new WaitUntil(() => Application.internetReachability == NetworkReachability.NotReachable);
            var waitInit = new WaitUntil(() => _api.Initialized);

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return waitWeb;

            yield return waitInit;
            yield return new WaitForSecondsRealtime(3f);

            SetLinks();
        }

        private async void SetLinks()
        {
            var linkSupport = await GetLink(key: SupportRmtKey);
            var linkAgreement = await GetLink(key: AgreementRmtKey);
            var linkPrivacy = await GetLink(key: PrivacyRmtKey);
            var linkSubscription = await GetLink(key: SubscriptionRmtKey);

            if (string.IsNullOrEmpty(linkSupport) == false)
                Support = linkSupport;

            if (string.IsNullOrEmpty(linkAgreement) == false)
                Agreement = linkAgreement;

            if (string.IsNullOrEmpty(linkPrivacy) == false)
                Privacy = linkPrivacy;

            if (string.IsNullOrEmpty(linkSubscription) == false)
                Subscription = linkSubscription;
        }

        private async Task<string> GetLink(string key)
        {
            var response = await _api.GetRemoteConfig(key);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
#if UNITY_EDITOR || TEST
                Debug.Log($"Remote config '{key}': " + response.body);
#endif
                return response.body;
            }
            else
            {
                Debug.LogWarning($"Fail to recieve remote config '{key}': " + response.statusCode);
                return string.Empty;
            }
        }
    }
}
