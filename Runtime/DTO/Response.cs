using UnityEngine;
using UnityEngine.Scripting;
using static UnityEngine.Networking.UnityWebRequest;

namespace AdsAppView.DTO
{
    [Preserve]
    public class Response
    {
        public Response(Result statusCode, string reasonPhrase, string body, bool isBase64Encoded, Texture2D texture)
        {
            this.statusCode = statusCode;
            this.body = body;
            this.reasonPhrase = reasonPhrase;
            this.isBase64Encoded = isBase64Encoded;
            this.texture = texture;
        }

        public Result statusCode { get; private set; }
        public string reasonPhrase { get; private set; }
        public string body { get; private set; }
        public bool isBase64Encoded { get; private set; }
        public Texture2D texture { get; private set; }
    }
}
