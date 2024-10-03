using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class SpriteData
    {
        public Sprite sprite;
        public string link;
        public string name;
        public float aspectRatio;
    }
}
