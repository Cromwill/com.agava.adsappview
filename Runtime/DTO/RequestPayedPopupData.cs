using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class RequestPayedPopupData
    {
        public string app_id { get; set; }
        public string store_id { get; set; }
        public string platform { get; set; }
        public bool vip { get; set; }
    }
}
