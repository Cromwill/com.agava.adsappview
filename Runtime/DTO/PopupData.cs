using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class PopupData
    {
        public Sprite background;
        public Sprite play_button;
        public byte[] body;
        public string link;
        public string name;
        public string path;
    }
}
