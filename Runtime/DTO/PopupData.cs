using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class PopupData
    {
        public byte[] bytes;
        public string link;
        public string name;
        public string path;
    }
}
