using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using AdsAppView.Utility;
using AdsAppView.DTO;
using Newtonsoft.Json;
using System;

namespace AdsAppView.Program
{
    public class Boot : MonoBehaviour
    {
        private const float DelayWhileAuthPluginInit = 10f;
#if UNITY_WEBGL
        private const string Platform = "webgl";
#elif UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "Android";
#elif UNITY_IOS
        private const string Platform = "iOS";
#endif

        [SerializeField] private Links _links;
        [SerializeField] private ViewPresenterConfigs _viewPresenterConfigs;
        [Header("Web settings")]
        [Tooltip("Bund for plugin settings")]
        [SerializeField] private BuildVersionHolder _buildVersionHolder;
        [Tooltip("Store name")]
        [SerializeField] private Store _storeName;
        [Tooltip("Server name remote data")]
        [SerializeField] private string _serverPath;
        [Tooltip("Assets settings")]
        [SerializeField] private bool _freeApp = true;
        [SerializeField] private bool _useAssetBundles = true;
        [SerializeField] private AssetsBundlesLoader _assetsBundlesLoader;
        [SerializeField] private GameObject _defaultAsset;

#if UNITY_WEBGL
        [Header("WEBGL")]
        [SerializeField] private string _appId;
#endif

#if UNITY_ANDROID || UNITY_IOS
        private string _appId => Application.identifier;
#endif

        private AdsAppAPI _api;
        private AppData _appData;
        private PreloadService _preloadService;

        public static Boot Instance { get; private set; }

        private IEnumerator Start()
        {
            if (_buildVersionHolder == null)
                throw new NullReferenceException("[Boot] Build version holder is null!!!");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_freeApp)
                yield return Construct(vip: false);
        }

        public IEnumerator Construct(bool vip)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            _api = new(_serverPath, _appId);
            _appData = new() { app_id = _appId, store_id = _storeName.ToString(), platform = Platform };
            _preloadService = new(_api, _buildVersionHolder.BundleId, _freeApp, vip, _appData, _storeName);
            Debug.Log("#Boot# " + JsonConvert.SerializeObject(_appData));

            yield return _preloadService.Preparing();

            if (_freeApp)
                yield return _links.Initialize(_api);

            yield return _viewPresenterConfigs.Initialize(_api);

            if (_preloadService.IsPluginAvailable)
                yield return Initialize(vip);
            else
                Debug.Log("#Boot# Popup plugin disabled");
        }

        private IEnumerator Initialize(bool vip)
        {
            AnalyticsService.SendStartApp(_appId);
            GameObject created = null;
            Debug.Log("#Boot# Popup plugin enabled");

            if (_useAssetBundles)
            {
                Task<GameObject> task = _assetsBundlesLoader.GetPopupObject();

                yield return new WaitUntil(() => task.IsCompleted);
                GameObject bundlePopupPrefab = task.Result;

                if (bundlePopupPrefab != null)
                {
                    created = Instantiate(bundlePopupPrefab);
                    created.name = "AssetBundle-PopupManager";
                    Debug.Log("#Boot# Created popup: " + created.name);
                }

                _assetsBundlesLoader.Unload();
            }

            if (created == null)
            {
                created = Instantiate(_defaultAsset);
                created.name = "Default-PopupManager";
                Debug.Log("#Boot# Default-PopupManager Instantiated");
            }

            yield return created.GetComponent<PopupManager>().Construct(_appData, _freeApp, vip);
        }

        public enum Store
        {
            AppStore,
            Google,
            Huawei,
            RuStore,
            test
        }
    }
}
