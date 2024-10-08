using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using AdsAppView.DTO;
using AdsAppView.Utility;
using Newtonsoft.Json;

namespace AdsAppView.Program
{
    [Serializable]
    public class AssetsBundlesLoader
    {
#if UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "Android";
#elif UNITY_IOS
        private const string Platform = "Ios";
#endif
        private class AssetPath
        {
            public string[] m_InternalIds;
        }

        private const string ControllerName = "AdsApp";
        private const string FtpCredsRCName = "ftp-creds";

        [SerializeField] private string _catalogPath;
        [SerializeField] private string _assetName;

        private AssetBundle _assetBundle;
        private string _filePath;

        public void Unload()
        {
            if (File.Exists(_filePath))
            {
                _assetBundle.UnloadAsync(false);
                File.Delete(_filePath);
            }
        }

        public async Task<GameObject> GetPopupObject()
        {
            Response ftpCredentialResponse = await AdsAppAPI.Instance.GetRemoteConfig(ControllerName, FtpCredsRCName);

            if (ftpCredentialResponse.statusCode == UnityWebRequest.Result.Success)
            {
                FtpCreds creds = JsonConvert.DeserializeObject<FtpCreds>(ftpCredentialResponse.body);

                if (creds == null)
                {
                    Debug.LogError("#AssetsBundlesLoader# Fail get creds data");
                    return null;
                }

                string cataloPath = $"{_catalogPath}/{Platform}/catalog_{Application.version}.json";
                Debug.Log("#AssetsBundlesLoader# Try download catalog: " + cataloPath);
                string pathFile = DownloadConfigFile(cataloPath, creds.login, creds.password);

                AssetPath path = JsonConvert.DeserializeObject<AssetPath>(pathFile);
                List<string> list = path.m_InternalIds.ToList();

                string assetPath = list.FirstOrDefault(s => s.StartsWith("http"));
                assetPath = assetPath.Replace("http", "ftp");
                Debug.Log("#AssetsBundlesLoader# " + assetPath);

                _assetBundle = DownloadAssetBundleFile(assetPath, savePath: Application.persistentDataPath, creds.login, creds.password);

                if (_assetBundle == null)
                {
                    Debug.LogError("#AssetsBundlesLoader# Fail load bundle: " + _assetName);
                    return null;
                }

                GameObject target = _assetBundle.LoadAsset<GameObject>(_assetName);

                if (target == null)
                    Debug.LogError("#AssetsBundlesLoader# Fail load obj from asset bundle: " + _assetName);

                return target;
            }
            else
            {
                return null;
            }
        }

        private string DownloadConfigFile(string ftpUrl, string userName, string password)
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

            using (Stream input = request.GetResponse().GetResponseStream())
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new())
                {
                    int read;

                    while (input.CanRead && (read = input.Read(buffer, 0, buffer.Length)) > 0)
                        ms.Write(buffer, 0, read);

                    return Encoding.Default.GetString(ms.ToArray());
                }
            }
        }

        private AssetBundle DownloadAssetBundleFile(string ftpUrl, string savePath, string userName, string password)
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

            ftpUrl = ftpUrl.Replace("ftp://ftp-p.ctcmedia.ru/mediartk/AssetsBundles/", "");
            ftpUrl = ftpUrl.Replace("/", "");
            ftpUrl = ftpUrl.Replace(Platform, "");
            Debug.Log("#AssetsBundlesLoader# Asset bundle name to load: " + ftpUrl);

            string fileName = ftpUrl;

            try
            {
                AssetBundle assetBundle = null;
                assetBundle = DownloadAndSave(request.GetResponse(), savePath, fileName);
                return assetBundle;
            }
            catch
            {
                Debug.LogError("#AssetsBundlesLoader# Fail to download asset bundle #DownloadAssetBundleFile()#: " + fileName);
                return null;
            }
        }

        private AssetBundle DownloadAndSave(WebResponse request, string savePath, string name)
        {
            savePath += "/Assets";

            if (Directory.Exists(savePath) == false)
            {
                Directory.CreateDirectory(savePath);
                Debug.Log($"#AssetsBundlesLoader# Created folder: " + savePath);
            }
            else
            {
                Debug.Log($"#AssetsBundlesLoader# Folder exist: " + savePath);
            }

            string path = string.IsNullOrEmpty(name) ? savePath : savePath + "/" + name;

            using (Stream reader = request.GetResponseStream())
            {
                _filePath = path;

                using (FileStream fileStream = new(path, FileMode.Create))
                {
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
            }

            Debug.Log($"#AssetsBundlesLoader# Try load resource {name} from: " + savePath);
            AssetBundle assetBundle = null;

            try
            {
                assetBundle = AssetBundle.LoadFromFile(path);

                if (assetBundle == null)
                {
                    Debug.Log("#AssetsBundlesLoader# Failed to load AssetBundle!");
                    return null;
                }

                Debug.Log($"#AssetsBundlesLoader# Loaded bundle web {assetBundle.GetAllAssetNames()[0]}");
                return assetBundle;
            }
            catch
            {
                Debug.LogError($"#AssetsBundlesLoader# Fail to load bundle web");
                return null;
            }
        }
    }
}
