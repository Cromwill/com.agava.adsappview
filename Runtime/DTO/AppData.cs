using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve]
    public class AppData
    {
        public string app_id { get; set; }
        public string store_id {  get; set; }
        public string platform { get; set; }
    }
}
