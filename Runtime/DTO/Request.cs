using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class Request
    {
        public string api_name { get; set; }
        public string body { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string login { get; set; }
        public string password { get; set; }
    }
}
