using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using AdsAppView.Utility;
using AdsAppView.DTO;
using Newtonsoft.Json;

namespace AdsAppView.Program
{
    public class Boot : MonoBehaviour
    {
#if UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "android";
#elif UNITY_IOS
        private const string Platform = "ios";
#endif

        [SerializeField] private Links _links;
        [Header("Web settings")]
        [Tooltip("Bund for plugin settings")]
        [SerializeField] private int _bundlIdVersion = 1;
        [Tooltip("Store name")]
        [SerializeField] private string _storeName;
        [Tooltip("Server name remote data")]
        [SerializeField] private string _serverPath;
        [Tooltip("Assets settings")]
        [SerializeField] private AssetsBundlesLoader _assetsBundlesLoader;
        [SerializeField] private GameObject _defaultAsset;

        private string _appId => Application.identifier;
        private AdsAppAPI _api;
        private AppData _appData;
        private PreloadService _preloadService;

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            _api = new(_serverPath, _appId);
            _appData = new() { app_id = _appId, store_id = _storeName, platform = Platform };
            _preloadService = new(AdsAppAPI.Instance, _bundlIdVersion);
            Debug.Log("#Boot# " + JsonConvert.SerializeObject(_appData));

            yield return _preloadService.Preparing();
            yield return _links.Initialize(AdsAppAPI.Instance);

            if (_preloadService.IsPluginAwailable)
                yield return Initialize();
            else
                Debug.Log("#Boot# Plugin disabled");
        }

        private IEnumerator Initialize()
        {
            Task<GameObject> task = _assetsBundlesLoader.GetPopupObject();

            yield return new WaitUntil(() => task.IsCompleted);
            GameObject bundlePopupPrefab = task.Result;
            GameObject created;

            if (bundlePopupPrefab != null)
            {
                created = Instantiate(bundlePopupPrefab);
                created.name = "AssetBundle-PopupManager";
                Debug.Log("#Boot# Creeated popup: " + created.name);
            }
            else
            {
                created = Instantiate(_defaultAsset);
                created.name = "Default-PopupManager";
                Debug.LogError("#Boot# Default-PopupManager Instantiated");
            }

            _assetsBundlesLoader.Unload();
            yield return created.GetComponent<PopupManager>().Construct(_appData);
        }
    }
}
