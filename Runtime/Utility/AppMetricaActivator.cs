using AdsAppView.DTO;
using Io.AppMetrica;
using UnityEngine;

namespace AdsAppView.Utility
{
    public static class AppMetricaActivator
    {
        private const string FirtsAppMetricaLaunch = nameof(FirtsAppMetricaLaunch);
        private const string TestKey = "e8883556-3e2e-4706-a0a5-a7eb677fe077";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Activate()
        {
            string key = TestKey;

            var info = Resources.Load<AppMetricaInfo>("AppMetricaInfo");

            if (info != null)
                key = info.Key;

            AppMetrica.Activate(new AppMetricaConfig(key)
            {
                FirstActivationAsUpdate = !IsFirstLaunch(),
            });
#if UNITY_EDITOR || TEST
            Debug.Log($"AppMetrica start: {key}");
#endif
        }

        private static bool IsFirstLaunch()
        {
            if (PlayerPrefs.HasKey(FirtsAppMetricaLaunch))
            {
                return false;
            }
            else
            {
                PlayerPrefs.SetString(FirtsAppMetricaLaunch, "true");
                return true;
            }
        }
    }

}
