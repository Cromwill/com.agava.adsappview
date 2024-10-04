using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using AdsAppView.DTO;
using Newtonsoft.Json;
using System.IO;
using AdsAppView.Utility;

namespace AdsAppView.Program
{
    public partial class PopupManager : MonoBehaviour
    {
#if UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "android";
#elif UNITY_IOS
        private const string Platform = "ios";
#endif
        private const string ControllerName = "AdsApp";
        private const string SettingsRCName = "app-settings";
        private const string FilePathRCName = "file-path";
        private const string FtpCredsRCName = "ftp-creds";
        private const string CarouselPicture = "picrure";
        private const string Caching = "caching";

        [Header("Web settings")]
        [Tooltip("Bund for plugin settings")]
        [SerializeField] private int _bundlIdVersion = 1;
        [Tooltip("Store name")]
        [SerializeField] private string _storeName;
        [Tooltip("Server name remote data")]
        [SerializeField] private string _serverPath;
        [Header("Components")]
        [SerializeField] private ViewPresenter _viewPresenter;
        [SerializeField] private Links _links;

        private AdsAppAPI _api;
        private AppData _appData;
        private AppSettingsData _settingsData;
        private AdsFilePathsData _adsFilePathsData;
        private PreloadService _preloadService;

        [SerializeField, HideInInspector] private List<SpriteData> _sprites;
        private SpriteData _sprite;

        private float _firstTimerSec = 60f;
        private float _regularTimerSec = 180f;
        private bool _caching = true;

        public string AppId => Application.identifier;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator Start()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            _appData = new() { app_id = AppId, store_id = _storeName, platform = Platform };
            Debug.Log(JsonConvert.SerializeObject(_appData));
            _api = new(_serverPath, AppId);
            _preloadService = new(_api, _bundlIdVersion);
            _viewPresenter.Construct(_links);

            yield return _preloadService.Preparing();
            yield return _links.Initialize(_api);

            if (_preloadService.IsPluginAwailable)
                StartView();
            else
                Debug.Log("Plugin disabled");
        }

        private async void StartView()
        {
            Response appSettingsResponse = await _api.GetAppSettings(ControllerName, SettingsRCName, _appData);

            if (appSettingsResponse.statusCode == UnityWebRequest.Result.Success)
            {
                AppSettingsData data = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsResponse.body);

                if (data != null)
                {
                    await SetCachingConfig();

                    _settingsData = data;
                    _firstTimerSec = data.first_timer;
                    _regularTimerSec = data.regular_timer;

                    _sprite = await GetSprite();

                    if (_sprite != null)
                        StartCoroutine(ShowingAds());

                    if (_settingsData.carousel)
                        await GetSprites();
                }
                else
                {
                    Debug.LogError("App settings is null");
                }
            }
            else
            {
                Debug.LogError("Fail to getting settings: " + appSettingsResponse.statusCode);
            }
        }

        private IEnumerator ShowingAds()
        {
            yield return new WaitForSecondsRealtime(_firstTimerSec);

            _viewPresenter.Show(_sprite);
            yield return new WaitWhile(() => _viewPresenter.Enable);

            if (_settingsData.carousel)
            {
                int index = 0;

                while (true)
                {
                    yield return new WaitForSecondsRealtime(_regularTimerSec);

                    _viewPresenter.Show(_sprites[index]);
                    yield return new WaitWhile(() => _viewPresenter.Enable);

                    index++;

                    if (index >= _sprites.Count)
                        index = 0;
                }
            }
            else
            {
                while (true)
                {
                    yield return new WaitForSecondsRealtime(_regularTimerSec);
                    _viewPresenter.Show(_sprite);
                }
            }
        }

        private async Task GetSprites()
        {
            for (int i = 0; i < _settingsData.carousel_count; i++)
            {
                SpriteData newSprite = await GetSprite(index: i);
                newSprite ??= _sprite;

                _sprites.Add(newSprite);
            }
        }

        private async Task<SpriteData> GetSprite(int index = -1)
        {
            string appId = index == -1 ? _settingsData.ads_app_id : CarouselPicture + index;
            AppData newData = new AppData() { app_id = appId, store_id = _storeName, platform = Platform };

            Response filePathResponse = await _api.GetRemoteConfig(ControllerName, FilePathRCName, newData);

            if (filePathResponse.statusCode == UnityWebRequest.Result.Success)
            {
                _adsFilePathsData = JsonConvert.DeserializeObject<AdsFilePathsData>(filePathResponse.body);

                if (_adsFilePathsData == null)
                    Debug.LogError("Fail get file path data");

                Response ftpCredentialResponse = await _api.GetRemoteConfig(ControllerName, FtpCredsRCName);

                if (ftpCredentialResponse.statusCode == UnityWebRequest.Result.Success)
                {
                    FtpCreds creds = JsonConvert.DeserializeObject<FtpCreds>(ftpCredentialResponse.body);

                    if (creds == null)
                    {
                        Debug.LogError("Fail get creds data");
                        return null;
                    }

                    string cacheTexturePath = ConstructCacheTexturePath(_adsFilePathsData);

                    if ((_caching && TryLoadCacheTexture(cacheTexturePath, out Texture2D texture)) == false)
                    {
                        Response textureResponse = _api.GetTextureData(creds.host, _adsFilePathsData.file_path, creds.login, creds.password);

                        if (textureResponse.statusCode == UnityWebRequest.Result.Success)
                        {
                            texture = textureResponse.texture;

                            if (_caching)
                                TrySaveCacheTexture(cacheTexturePath, texture);
                        }
                        else
                        {
                            Debug.LogError("Fail to download texture: " + textureResponse.statusCode);
                            return null;
                        }
                    }

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    return new SpriteData() { sprite = sprite, link = _adsFilePathsData.app_link, name = _adsFilePathsData.file_path, aspectRatio = (float)texture.width / texture.height };
                }
                else
                {
                    Debug.LogError("Fail to getting ftp creds: " + ftpCredentialResponse.statusCode);
                    return null;
                }
            }
            else
            {
                Debug.LogError("Fail to getting file path: " + filePathResponse.statusCode);
                return null;
            }
        }

        private async Task SetCachingConfig()
        {
            Response cachingResponse = await _api.GetRemoteConfig(Caching);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (bool.TryParse(body, out bool caching))
                    _caching = caching;
            }
        }

        private string ConstructCacheTexturePath(AdsFilePathsData adsFilePathsData)
        {
            string extension = Path.GetExtension(adsFilePathsData.file_path);
            return Path.Combine(Application.persistentDataPath, adsFilePathsData.ads_app_id + extension);
        }

        private bool TryLoadCacheTexture(string cacheFilePath, out Texture2D texture)
        {
            texture = null;

            if (File.Exists(cacheFilePath))
            {
                byte[] rawData = File.ReadAllBytes(cacheFilePath);
                texture = new Texture2D(2, 2);
                texture.LoadImage(rawData);

#if UNITY_EDITOR
                Debug.Log($"Cache texture loaded from path: {cacheFilePath}");
#endif
            }

            return texture != null;
        }

        private void TrySaveCacheTexture(string cacheFilePath, Texture2D texture)
        {
            try
            {
                File.WriteAllBytes(cacheFilePath, texture.EncodeToPNG());

#if UNITY_EDITOR
                Debug.Log($"Cache texture saved to path: {cacheFilePath}");
#endif
            }
            catch (IOException exception)
            {
#if UNITY_EDITOR
                Debug.LogError("Fail to save cache texture: " + exception.Message);
#endif
            }
        }
    }

    public partial class PopupManager : MonoBehaviour
    {
        #region TEST
#if UNITY_EDITOR
        [Header("Test"), Tooltip("Can be null")]
        [SerializeField] private Button _testGetTextureBtn;

        private void OnEnable()
        {
            if (_testGetTextureBtn != null)
                _testGetTextureBtn.onClick.AddListener(GetTexture);
        }

        private void OnDisable()
        {
            if (_testGetTextureBtn != null)
                _testGetTextureBtn.onClick.RemoveListener(GetTexture);
        }

        private async void GetTexture()
        {
            SpriteData sprite = await GetSprite();
            _viewPresenter.Show(sprite);
        }
#endif
        #endregion
    }
}
