using System;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using AdsAppView.DTO;
using Newtonsoft.Json;

namespace AdsAppView.Utility
{
    [Preserve]
    public class AdsAppAPI
    {
        private WebClient _webClient;
        private string _appId;

        public bool Initialized => _webClient != null;

        public static AdsAppAPI Instance;

        public AdsAppAPI(string serverPath, string appId)
        {
            if (string.IsNullOrEmpty(serverPath))
                throw new InvalidOperationException(nameof(AdsAppAPI) + " Ip not entered");

            if (string.IsNullOrEmpty(appId))
                throw new InvalidOperationException(nameof(AdsAppAPI) + " appId not entered");

            if (Initialized)
                throw new InvalidOperationException(nameof(AdsAppAPI) + " has already been initialized");

            _appId = appId;
            _webClient = new WebClient(serverPath);
            Instance = this;
        }

        public async Task<Response> GetRemoteConfig(string remoteName)
        {
            EnsureInitialize();
            return await _webClient.GetRemote("Remoteconfig", remoteName);
        }

        public async Task<Response> GetPluginSettings(string remoteName)
        {
            EnsureInitialize();
            return await _webClient.GetPluginSettings("RemoteConfig", remoteName);
        }

        public async Task<Response> GetRemoteConfig(string controllerName, string apiName)
        {
            EnsureInitialize();

            var request = new Request()
            {
                api_name = controllerName + "/" + apiName,
            };

            return await _webClient.GetRemote(request);
        }

        public async Task<Response> GetRemoteConfig(string controllerName, string apiName, AppData data)
        {
            EnsureInitialize();

            var request = new Request()
            {
                api_name = controllerName + "/" + apiName,
                body = JsonConvert.SerializeObject(data),
            };

            return await _webClient.GetRemote(request);
        }

        public async Task<Response> GetFilePath(string controllerName, string apiName, AppData data)
        {
            EnsureInitialize();

            var request = new Request()
            {
                api_name = controllerName + "/" + apiName,
                body = JsonConvert.SerializeObject(data),
            };

            return await _webClient.GetFilePath(request);
        }

        public async Task<Response> GetAppSettings(string controllerName, string apiName, AppData data)
        {
            EnsureInitialize();

            var request = new Request()
            {
                api_name = controllerName + "/" + apiName,
                body = JsonConvert.SerializeObject(data),
            };

            return await _webClient.GetAppSettings(request);
        }

        /// <summary>
        /// Send ftp request to get bytes data
        /// </summary>
        /// <param name="host">Need data ftp server name/login/pass</param>
        /// <returns></returns>
        public Response GetBytesData(string host, string filePath, string login, string password)
        {
            EnsureInitialize();
            string path = host + "/" + filePath;

            var request = new Request()
            {
                api_name = path,
                login = login,
                password = password,
            };

            return _webClient.GetBytesData(request);
        }

        private void EnsureInitialize()
        {
            if (Initialized == false)
                throw new InvalidOperationException(nameof(AdsAppAPI) + " is not initialized");
        }
    }
}
