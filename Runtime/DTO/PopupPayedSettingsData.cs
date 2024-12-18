using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class PopupPayedSettingsData
    {
        public string app_id { get; set; }
        public string store_id { get; set; }
        public string platform { get; set; }
        public int released_version { get; set; }
        public bool released_state { get; set; }
        public int review_version { get; set; }
        public bool review_state { get; set; }
        public bool vip_state { get; set; }
    }
}
