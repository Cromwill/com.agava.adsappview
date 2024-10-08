using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using AdsAppView.DTO;
using AdsAppView.Utility;
using Newtonsoft.Json;

namespace AdsAppView.Program
{
    public class PopupManager : MonoBehaviour
    {
        private const string ControllerName = "AdsApp";
        private const string SettingsRCName = "app-settings";
        private const string FilePathRCName = "file-path";
        private const string FtpCredsRCName = "ftp-creds";
        private const string CarouselPicture = "picrure";
        private const string Caching = "caching";
        private const string ClosingDelay = "closing-delay";
        private const string EnablingTime = "enabling-time";

        [SerializeField] private ViewPresenter _viewPresenter;

        private AppData _appData;
        private AppSettingsData _settingsData;
        private AdsFilePathsData _adsFilePathsData;

        private readonly List<SpriteData> _sprites = new();
        private SpriteData _sprite;

        private float _firstTimerSec = 60f;
        private float _regularTimerSec = 180f;
        private float _closingDelay = 2;
        private float _enablingTime = 2;
        private bool _caching = false;

        public IEnumerator Construct(AppData appData)
        {
            DontDestroyOnLoad(gameObject);
            _appData = appData;

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            StartView();
        }

        private async void StartView()
        {
            Response appSettingsResponse = await AdsAppAPI.Instance.GetAppSettings(ControllerName, SettingsRCName, _appData);

            if (appSettingsResponse.statusCode == UnityWebRequest.Result.Success)
            {
                AppSettingsData data = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsResponse.body);

                if (data != null)
                {
                    await SetEnablingTimeConfig();
                    await SetClosingDelayConfig();
                    await SetCachingConfig();

                    _viewPresenter.Initialize(_enablingTime, _closingDelay);

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
                    yield return new WaitWhile(() => _viewPresenter.Enable);
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
            AppData newData = new AppData() { app_id = appId, store_id = _appData.store_id, platform = _appData.platform};

            Response filePathResponse = await AdsAppAPI.Instance.GetFilePath(ControllerName, FilePathRCName, newData);

            if (filePathResponse.statusCode == UnityWebRequest.Result.Success)
            {
                _adsFilePathsData = JsonConvert.DeserializeObject<AdsFilePathsData>(filePathResponse.body);

                if (_adsFilePathsData == null)
                    Debug.LogError("Fail get file path data");

                Response ftpCredentialResponse = await AdsAppAPI.Instance.GetRemoteConfig(ControllerName, FtpCredsRCName);

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
                        Response textureResponse = AdsAppAPI.Instance.GetTextureData(creds.host, _adsFilePathsData.file_path, creds.login, creds.password);

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
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(Caching);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (bool.TryParse(body, out bool caching))
                {
                    _caching = caching;

#if UNITY_EDITOR
                    Debug.Log("Caching set to: " + _caching);
#endif
                }
            }
        }

        private async Task SetClosingDelayConfig()
        {
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(ClosingDelay);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (float.TryParse(body, out float closingDelay))
                {
                    _closingDelay = closingDelay;

#if UNITY_EDITOR
                    Debug.Log("Closing delay set to: " + _closingDelay);
#endif
                }
            }
        }

        private async Task SetEnablingTimeConfig()
        {
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(EnablingTime);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (float.TryParse(body, out float enablingTime))
                {
                    _enablingTime = enablingTime;

#if UNITY_EDITOR
                    Debug.Log("Enabling time set to: " + _enablingTime);
#endif
                }
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
}
