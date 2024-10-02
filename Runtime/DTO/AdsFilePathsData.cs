using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve]
    public class AdsFilePathsData
    {
        public string ads_app_id { get; set; }
        public string file_path { get; set; }
        public string app_link { get; set; }
    }
}
