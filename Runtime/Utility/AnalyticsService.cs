using Io.AppMetrica;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace AdsAppView.Utility
{
    [Preserve]
    public static class AnalyticsService
    {
        public static void SendStartApp(string appId) => AppMetrica.ReportEvent("App run", GetDataJson("App run", appId));
        public static void SendPopupView(string popupId) => AppMetrica.ReportEvent("Popup view", GetDataJson("Popup view", popupId));
        public static void SendPopupClosed() => AppMetrica.ReportEvent("Popup closed", GetDataJson($"Popup closed", "Close"));
        public static void SendPopupRedirectClick(string popupId, int count)
        {
            AppMetrica.ReportEvent("Popup redirect click", GetCountedDataJson("Popup redirect click", popupId, count));
            Debug.Log("#Analytics# Send Popup Redirect event");
        }

        private static string GetDataJson(string name, string value)
        {
            Data data = new Data()
            {
                Name = name,
                Value = value,
            };

            return JsonConvert.SerializeObject(data);
        }

        private static string GetCountedDataJson(string name, string value, int count)
        {
            DataCounted data = new DataCounted()
            {
                Name = name,
                Value = value,
                Count = count,
            };

            return JsonConvert.SerializeObject(data);
        }

        internal class Data
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        internal class DataCounted
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public int Count { get; set; }
        }
    }

}
