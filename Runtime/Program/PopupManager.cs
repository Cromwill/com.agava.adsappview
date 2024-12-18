using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using AdsAppView.DTO;
using AdsAppView.Utility;
using Newtonsoft.Json;
using System;
using Codice.Client.Common;

namespace AdsAppView.Program
{
    public class PopupManager : MonoBehaviour
    {
        private const string ControllerName = "AdsApp";
        private const string SettingsRCName = "app-settings";
        private const string PayedSettingsRCName = "popup-payed-settings";
        private const string PayedConfigRCName = "popup-payed-configs";
        private const string FilePathRCName = "file-path";
        private const string FtpCredsRCName = "ftp-creds";
        private const string CarouselPicture = "picrure";
        private const string Caching = "caching";
        private const int RetryCount = 3;
        private const int RetryDelayMlsec = 30000;

        [SerializeField] private ViewPresenterFactory _viewPresenterFactory;
        [SerializeField] private GamePause _gamePause;

        private IViewPresenter _viewPresenter;

        private AppData _appData;
        private AppSettingsData _freeAppConfigData;
        private PopupPayedConfigsData _payedConfigData;
        private AdsFilePathsData _adsFilePathsData;

        private readonly List<PopupData> _popupDataList = new();
        private PopupData _popupData;

        private float _firstTimerSec = 60f;
        private float _regularTimerSec = 180f;
        private bool _caching = false;

        private bool _vip = false;
        private bool _isPayedPopupRoutineWorked = false;
        private int _indexPopupCarosel = 0;

        public static PopupManager Instance { get; private set; }

        public IEnumerator Construct(AppData appData, bool freeApp, bool vip)
        {
            Instance = this;
            _vip = vip;
            _viewPresenter = _viewPresenterFactory.InstantiateViewPresenter(ViewPresenterConfigs.ViewPresenterType);

            _gamePause.Initialize(_viewPresenter);

            DontDestroyOnLoad(gameObject);
            _appData = appData;

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            StartView(freeApp);
        }

        public void ShowPopupPayedApp() => StartCoroutine(ShowingPopupPayedApp());

        private async void StartView(bool freeApp)
        {
            Response appSettingsPayedResponse;
            Response appSettingsCommonResponse;

            if (freeApp)
            {
                appSettingsCommonResponse = await AdsAppAPI.Instance.GetAppSettings(ControllerName, SettingsRCName, _appData);

                if (appSettingsCommonResponse.statusCode == UnityWebRequest.Result.Success)
                {
                    AppSettingsData data = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsCommonResponse.body);

                    if (data != null)
                    {
                        await SetCachingConfig();

                        _freeAppConfigData = data;
                        _firstTimerSec = data.first_timer;
                        _regularTimerSec = data.regular_timer;

                        _popupData = await GetPopupData();

                        if (_popupData != null)
                            StartCoroutine(ShowingAdsFreeApp());
                        else
                            Debug.LogError("#PopupManager# Fail get popup datas");

                        if (_freeAppConfigData.carousel)
                            await FillPopupDataList();
                    }
                    else
                    {
                        Debug.LogError("#PopupManager# App settings is null");
                    }
                }
                else
                {
                    Debug.LogError("#PopupManager# Fail to getting settings: " + appSettingsCommonResponse.statusCode);
                }
            }
            else //Payed popup initializing
            {
                RequestPayedPopupData requestData = new() { app_id = _appData.app_id, platform = _appData.platform, store_id = _appData.store_id, vip = _vip };
                appSettingsPayedResponse = await AdsAppAPI.Instance.GetAppSettings(ControllerName, PayedConfigRCName, requestData);

                if (appSettingsPayedResponse.statusCode == UnityWebRequest.Result.Success)
                {
                    PopupPayedConfigsData data = JsonConvert.DeserializeObject<PopupPayedConfigsData>(appSettingsPayedResponse.body);

                    if (data != null)
                    {
                        await SetCachingConfig();

                        _freeAppConfigData = new()
                        {
                            app_id = data.app_id,
                            platform = data.platform,
                            store_id = data.store_id,
                            ads_app_id = data.ads_app_id,
                            first_timer = data.first_timer,
                            regular_timer = data.regular_timer,
                            carousel = data.carousel,
                            carousel_count = data.carousel_count
                        };

                        _payedConfigData = data;
                        _firstTimerSec = data.first_timer;
                        _regularTimerSec = data.regular_timer;

                        _popupData = await GetPopupData();

                        if (_popupData == null)
                        {
                            Debug.LogError("#PopupManager# Fail get popup datas");
                        }
                        else
                        {
                            StartCoroutine(Waiting(_firstTimerSec));
                            IEnumerator Waiting(float time)
                            {
                                _isPayedPopupRoutineWorked = true;
                                yield return new WaitForSecondsRealtime(time);
                                _isPayedPopupRoutineWorked = false;
                            }
                        }

                        if (_payedConfigData.carousel)
                            await FillPopupDataList();
                    }
                    else
                    {
                        Debug.LogError("#PopupManager# App payed settings is null");
                    }
                }
                else
                {
                    Debug.LogError("#PopupManager# Fail to getting payed settings: " + appSettingsPayedResponse.statusCode);
                }
            }
        }

        private IEnumerator ShowingPopupPayedApp()
        {
            if (_isPayedPopupRoutineWorked)
                yield break;

            if (_payedConfigData.carousel)
            {
                _isPayedPopupRoutineWorked = true;
                yield return ShowingPopup(_regularTimerSec, _popupDataList[_indexPopupCarosel]);

                _indexPopupCarosel++;

                if (_indexPopupCarosel >= _popupDataList.Count)
                    _indexPopupCarosel = 0;
            }
            else
            {
                _isPayedPopupRoutineWorked = true;
                yield return ShowingPopup(_regularTimerSec, _popupData);
            }

            IEnumerator ShowingPopup(float time, PopupData popupData)
            {
                _viewPresenter.Show(popupData);
                AnalyticsService.SendPopupView(popupData.name);
                yield return new WaitWhile(() => _viewPresenter.Enable);
                yield return new WaitForSecondsRealtime(time);
                _isPayedPopupRoutineWorked = false;
            }
        }

        private IEnumerator ShowingAdsFreeApp()
        {
            IEnumerator ShowingPopup(float time, PopupData popupData)
            {
                yield return new WaitForSecondsRealtime(time);
                _viewPresenter.Show(popupData);
                AnalyticsService.SendPopupView(popupData.name);
                yield return new WaitWhile(() => _viewPresenter.Enable);
            }

            yield return ShowingPopup(_firstTimerSec, _popupData);

            if (_freeAppConfigData.carousel)
            {
                int index = 0;

                while (true)
                {
                    yield return ShowingPopup(_regularTimerSec, _popupDataList[index]);

                    index++;

                    if (index >= _popupDataList.Count)
                        index = 0;
                }
            }
            else
            {
                while (true)
                {
                    yield return ShowingPopup(_regularTimerSec, _popupData);
                }
            }
        }

        private async Task FillPopupDataList()
        {
            for (int i = 0; i < _freeAppConfigData.carousel_count; i++)
            {
                PopupData newSprite = null;

                for (int s = 0; s < RetryCount; s++)
                {
                    newSprite = await GetPopupData(index: i);

                    if (newSprite != null)
                        break;

                    await Task.Delay(RetryDelayMlsec);
                }

                newSprite ??= _popupData;
                _popupDataList.Add(newSprite);
            }
        }

        private async Task<PopupData> GetPopupData(int index = -1)
        {
            string appId = index == -1 ? _freeAppConfigData.ads_app_id : CarouselPicture + index;
            AppData newData = new() { app_id = appId, store_id = _appData.store_id, platform = _appData.platform };

            Response filePathResponse = await AdsAppAPI.Instance.GetFilePath(ControllerName, FilePathRCName, newData);

            if (filePathResponse.statusCode == UnityWebRequest.Result.Success)
            {
                _adsFilePathsData = JsonConvert.DeserializeObject<AdsFilePathsData>(filePathResponse.body);

                if (_adsFilePathsData == null)
                    Debug.LogError("#PopupManager# Fail get file path data");

                Response ftpCredentialResponse = await AdsAppAPI.Instance.GetRemoteConfig(ControllerName, FtpCredsRCName);

                if (ftpCredentialResponse.statusCode == UnityWebRequest.Result.Success)
                {
                    FtpCreds creds = JsonConvert.DeserializeObject<FtpCreds>(ftpCredentialResponse.body);

                    if (creds == null)
                    {
                        Debug.LogError("#PopupManager# Fail get creds data");
                        return null;
                    }

                    string cacheFilePath = FileUtils.ConstructFilePath(_adsFilePathsData.file_path, _adsFilePathsData.ads_app_id);

                    if ((_caching && FileUtils.TryLoadFile(cacheFilePath, out byte[] bytes)) == false)
                    {
                        Response textureResponse = AdsAppAPI.Instance.GetBytesData(creds.host, _adsFilePathsData.file_path, creds.login, creds.password);

                        if (textureResponse.statusCode == UnityWebRequest.Result.Success)
                        {
                            bytes = textureResponse.bytes;
                            FileUtils.TrySaveFile(cacheFilePath, bytes);
                        }
                        else
                        {
                            Debug.LogError("#PopupManager# Fail to download texture: " + textureResponse.statusCode);
                            return null;
                        }
                    }

                    return new PopupData() { bytes = bytes, link = _adsFilePathsData.app_link, name = _adsFilePathsData.file_path, path = cacheFilePath };
                }
                else
                {
                    Debug.LogError("#PopupManager# Fail to getting ftp creds: " + ftpCredentialResponse.statusCode);
                    return null;
                }
            }
            else
            {
                Debug.LogError("#PopupManager# Fail to getting file path: " + filePathResponse.statusCode);
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
                    Debug.Log("#PopupManager# Caching set to: " + _caching);
#endif
                }
            }
            else
            {
                Debug.LogError("#PopupManager# Fail to Set Caching Config whith error: " + cachingResponse.statusCode);
            }
        }
#if UNITY_EDITOR
        [ContextMenu("Show popup")]
        private void Show() => StartCoroutine(ShowingPopupPayedApp());
#endif
    }
}
