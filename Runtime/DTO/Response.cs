using System;
using UnityEngine;
using UnityEngine.Scripting;
using static UnityEngine.Networking.UnityWebRequest;

namespace AdsAppView.DTO
{
    [Preserve, Serializable]
    public class Response
    {
        public Response(Result statusCode, string reasonPhrase, string body, bool isBase64Encoded, byte[] bytes)
        {
            this.statusCode = statusCode;
            this.body = body;
            this.reasonPhrase = reasonPhrase;
            this.isBase64Encoded = isBase64Encoded;
            this.bytes = bytes;
        }

        public Result statusCode { get; private set; }
        public string reasonPhrase { get; private set; }
        public string body { get; private set; }
        public bool isBase64Encoded { get; private set; }
        public byte[] bytes { get; private set; }
    }
}
