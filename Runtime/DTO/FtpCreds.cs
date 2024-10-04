using System;
using UnityEngine.Scripting;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class FtpCreds
    {
        public string name { get; set; }
        public string host { get; set; }
        public string login { get; set; }
        public string password { get; set; }
    }
}
