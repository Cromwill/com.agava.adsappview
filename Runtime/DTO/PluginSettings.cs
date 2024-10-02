using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    /// <summary>
    ///     Plugin settings data.
    /// </summary>
    [Preserve, Serializable]
    public class PluginSettings
    {
        public string app_name;
        public string platform;
        public int released_version;
        public int review_version;
        public string plugin_state;
        public string test_review;
        public string common;
    }
}
