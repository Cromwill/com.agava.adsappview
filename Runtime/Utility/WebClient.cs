using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using Newtonsoft.Json;
using AdsAppView.DTO;
using System.IO;
using System.Net;

namespace AdsAppView.Utility
{
    [Preserve]
    public class WebClient
    {
        private const string RootApi = "/api";
        private const string AppJson = "application/json";
        private const string ContentType = "Content-Type";
        private const int TimeOut = 59;

        private readonly string _serverPath;

        protected enum RequestType { POST, GET, PUT }

        public WebClient(string serverPath) => _serverPath = serverPath;

        public async Task<Response> GetRemote(string apiName, string key)
        {
            string path = $"{GetHttpPath(apiName, key.ToLower())}";

            using (UnityWebRequest webRequest = CreateWebRequest(path, RequestType.GET))
            {
                webRequest.SendWebRequest();

                await WaitProccessing(webRequest);
                TryShowRequestInfo(webRequest, apiName);

                var body = JsonConvert.DeserializeObject<string>(webRequest.downloadHandler.text);
                return new Response(webRequest.result, webRequest.result.ToString(), body, false, null);
            }
        }

        public async Task<Response> GetRemote(Request request)
        {
            string path = $"{GetHttpPath(request.api_name)}";

            using (UnityWebRequest webRequest = CreateWebRequest(path, RequestType.GET, uploadBody: request.body))
            {
                webRequest.SendWebRequest();

                await WaitProccessing(webRequest);
                TryShowRequestInfo(webRequest, request.api_name);

                var body = webRequest.downloadHandler.text;
                return new Response(webRequest.result, webRequest.result.ToString(), body, false, null);
            }
        }

        public async Task<Response> GetPluginSettings(string apiName, string key)
        {
            string path = $"{GetHttpPath(apiName, key.ToLower())}";

            using (UnityWebRequest webRequest = CreateWebRequest(path, RequestType.GET))
            {
                webRequest.SendWebRequest();

                await WaitProccessing(webRequest);
                TryShowRequestInfo(webRequest, apiName);

                string body = webRequest.downloadHandler.text;
                return new Response(webRequest.result, webRequest.result.ToString(), body, false, null);
            }
        }

        public Response GetTextureData(Request request)
        {
            string address = request.api_name;
            Debug.Log(address);

            byte[] downloaded = DownloadWithFTP(address, savePath: string.Empty, request.login, request.password);
            
            byte[] DownloadWithFTP(string ftpUrl, string savePath = "", string userName = "", string password = "")
            {
                if (Uri.TryCreate(ftpUrl, UriKind.Absolute, out Uri uri) == false)
                    throw new NullReferenceException("Cant create uri: " + ftpUrl);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);

                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = true;

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    request.Credentials = new NetworkCredential(userName, password);

                request.Method = WebRequestMethods.Ftp.DownloadFile;

                if (!string.IsNullOrEmpty(savePath))
                {
                    DownloadAndSave(request.GetResponse(), savePath);
                    return null;
                }
                else
                {
                    return DownloadAsbyteArray(request.GetResponse());
                }
            }

            byte[] DownloadAsbyteArray(WebResponse request)
            {
                using (Stream input = request.GetResponseStream())
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while (input.CanRead && (read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
            }

            void DownloadAndSave(WebResponse request, string savePath)
            {
                Stream reader = request.GetResponseStream();

                if (Directory.Exists(Path.GetDirectoryName(savePath)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                FileStream fileStream = new FileStream(savePath, FileMode.Create);

                int bytesRead = 0;
                byte[] buffer = new byte[2048];

                while (true)
                {
                    bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        break;

                    fileStream.Write(buffer, 0, bytesRead);
                }

                fileStream.Close();
            }

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(downloaded);

            UnityWebRequest.Result result = texture != null ? UnityWebRequest.Result.Success : UnityWebRequest.Result.DataProcessingError;

            return new Response(result, result.ToString(), "", false, texture);
        }

        private string GetHttpPath(string apiName, string apiData = null, bool api = true)
        {
            apiData ??= string.Empty;
            string apiRoute = string.Empty;

            if (api)
                apiRoute = RootApi;

            string path = $"{_serverPath}{apiRoute}/{apiName.ToLower()}/{apiData}";
            return $"https://{path}";
        }

        private UnityWebRequest CreateWebRequest(string path, RequestType type, string accessToken = null, string uploadBody = null, bool timeOut = true)
        {
            var httpRequest = new UnityWebRequest(path, type.ToString());
            httpRequest.downloadHandler = new DownloadHandlerBuffer();

            if (timeOut)
                httpRequest.timeout = TimeOut;

            if (string.IsNullOrEmpty(uploadBody) == false)
                httpRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(uploadBody));

            httpRequest.SetRequestHeader(ContentType, AppJson);

            if (string.IsNullOrEmpty(accessToken) == false)
                httpRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            return httpRequest;
        }

        private async Task WaitProccessing(UnityWebRequest webRequest, Action<float> progress = null)
        {
            while (webRequest.result == UnityWebRequest.Result.InProgress)
            {
                progress?.Invoke(webRequest.downloadProgress);
                await Task.Yield();
            }
        }

        private void TryShowRequestInfo(UnityWebRequest webRequest, string method)
        {
            Debug.Log($"response {method} to {webRequest.url} done {webRequest.result}. Result: {webRequest.downloadHandler.text}");

            if (webRequest.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Response {method} fail: {webRequest.error}, {webRequest.result}");
        }
    }
}
