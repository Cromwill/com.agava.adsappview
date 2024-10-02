using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using AdsAppView.DTO;
using Newtonsoft.Json;

namespace AdsAppView.Utility
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

        [SerializeField] private List<SpriteData> _sprites;
        private SpriteData _sprite;

        private float _firstTimerSec = 60f;
        private float _regularTimerSec = 180f;

        public string AppId => Application.identifier;

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
            Response appSettingsResponse = await _api.GetRemoteConfig(ControllerName, SettingsRCName, _appData);

            if (appSettingsResponse.statusCode == UnityWebRequest.Result.Success)
            {
                AppSettingsData data = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsResponse.body);

                if (data != null)
                {
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

            if (_settingsData.carousel)
            {
                int index = 0;
                while (true)
                {
                    yield return new WaitForSecondsRealtime(_regularTimerSec);
                    _viewPresenter.Show(_sprites[index]);
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

                if (newSprite != null)
                    _sprites.Add(newSprite);
                else
                    _sprites.Add(_sprite);
            }
        }

        private async Task<SpriteData> GetSprite(int index = -1)
        {
            AppData newData;

            if (index == -1)
                newData = new AppData() { app_id = _settingsData.ads_app_id, store_id = _storeName, platform = Platform };
            else
                newData = new AppData() { app_id = CarouselPicture + index, store_id = _storeName, platform = Platform };

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
                        Debug.LogError("Fail get creds data");

                    Response textureResponse = _api.GetTextureData(creds.host, _adsFilePathsData.file_path, creds.login, creds.password);

                    if (textureResponse.statusCode == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = textureResponse.texture;
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        return new SpriteData() { sprite = sprite, link = _adsFilePathsData.app_link, name = _adsFilePathsData.file_path };
                    }
                    else
                    {
                        Debug.LogError("Fail to download texture: " + textureResponse.statusCode);
                        return null;
                    }
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
    }

    public partial class PopupManager : MonoBehaviour
    {
        #region TEST
#if UNITY_EDITOR
        [SerializeField] private Button _testGetTextureBtn;

        private void Awake()
        {
            if (_testGetTextureBtn != null)
                _testGetTextureBtn.onClick.AddListener(GetTexture);
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
