using Io.AppMetrica;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace AdsAppView.Utility
{
    [Preserve]
    public static class AnalyticsService
    {
        public static void SendStartApp(string appId) => AppMetrica.ReportEvent("App run", GetJson("App run", appId));

        private static string GetJson(string name, string value)
        {
            Data data = new Data()
            {
                Name = name,
                Value = value
            };

            return JsonConvert.SerializeObject(data);
        }

        internal class Data
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }

}
