using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class PopupSourceLinkData
    {
        public string app_id;
        public string source_link;
        public string platform;
    }
}
