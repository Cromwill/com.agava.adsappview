/*
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WebView : MonoBehaviour
{
    [SerializeField] private WebViewObject _webViewObject;
    [SerializeField] private RectTransform _rectTransform;

    private void Start()
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.canvas = GameObject.Find("Canvas");
#endif

        _webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log(string.Format("CallFromJS[{0}]", msg));
            },
            err: (msg) =>
            {
                Debug.Log(string.Format("CallOnError[{0}]", msg));
            },
            httpErr: (msg) =>
            {
                Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
            },
            started: (msg) =>
            {
                Debug.Log(string.Format("CallOnStarted[{0}]", msg));
            },
            hooked: (msg) =>
            {
                Debug.Log(string.Format("CallOnHooked[{0}]", msg));
            },
            cookies: (msg) =>
            {
                Debug.Log(string.Format("CallOnCookies[{0}]", msg));
            },
            ld: (msg) =>
            {
                Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
                // NOTE: the following js definition is required only for UIWebView; if
                // enabledWKWebView is true and runtime has WKWebView, Unity.call is defined
                // directly by the native plugin.
#if true
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                window.location = 'unity:' + msg;
                            }
                        };
                    }
                ";
#else
                // NOTE: depending on the situation, you might prefer this 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                var iframe = document.createElement('IFRAME');
                                iframe.setAttribute('src', 'unity:' + msg);
                                document.documentElement.appendChild(iframe);
                                iframe.parentNode.removeChild(iframe);
                                iframe = null;
                            }
                        };
                    }
                ";
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                var js = @"
                    window.Unity = {
                        call:function(msg) {
                            parent.unityWebView.sendMessage('WebViewObject', msg);
                        }
                    };
                ";
#else
                var js = "";
#endif
                _webViewObject.EvaluateJS(js + @"Unity.call('ua=' + navigator.userAgent)");
            },
            transparent: false,
            zoom: true,
            ua: "custom user agent string",
            radius: 0,  // rounded corner radius in pixel
                        // android
            androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
                                      // ios
            enableWKWebView: true,
            wkContentMode: 1,  // 0: recommended, 1: mobile, 2: desktop
            wkAllowsLinkPreview: true,
            // editor
            separated: false
            );

        // cf. https://github.com/gree/unity-webview/issues/1094#issuecomment-2358718029

    }

    public IEnumerator ShowUrl(string url)
    {
        while (!_webViewObject.IsInitialized())
        {
            yield return null;
        }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
        webViewObject.devicePixelRatio = 1;  // 1 or 2
#endif
        // cf. https://github.com/gree/unity-webview/pull/512
        // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
        //webViewObject.SetAlertDialogEnabled(false);

        // cf. https://github.com/gree/unity-webview/pull/728
        //webViewObject.SetCameraAccess(true);
        //webViewObject.SetMicrophoneAccess(true);

        // cf. https://github.com/gree/unity-webview/pull/550
        // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        // cf. https://github.com/gree/unity-webview/pull/570
        // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
        //webViewObject.SetBasicAuthInfo("id", "password");

        _webViewObject.SetScrollbarsVisibility(false);

        Vector2 offsetMin = _rectTransform.offsetMin;
        Vector2 offsetMax = _rectTransform.offsetMax;

        _webViewObject.SetMargins((int)offsetMin.x, (int)-offsetMax.y, (int)-offsetMax.x, (int)offsetMin.y);
        _webViewObject.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
        //webViewObject.SetMixedContentMode(2);  // android only. 0: MIXED_CONTENT_ALWAYS_ALLOW, 1: MIXED_CONTENT_NEVER_ALLOW, 2: MIXED_CONTENT_COMPATIBILITY_MODE

        _webViewObject.SetVisibility(true);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (url.StartsWith("http"))
        {
            _webViewObject.LoadURL(url.Replace(" ", "%20"));
        }
        else
        {
            string directoryPath = Path.GetDirectoryName(url);
            string directoryName = Path.GetFileNameWithoutExtension(directoryPath);
            string htmlFileName = Path.GetFileName(url);
            string htmlFilePath = url;

            foreach (string filename in Directory.GetFiles(directoryPath))
            {
                var src = Path.GetFullPath(filename);
                var dst = Path.Combine(Application.temporaryCachePath, directoryName, Path.GetFileName(filename));
                byte[] result = null;

                string ext = Path.GetExtension(filename);

                if (src.Contains("://"))
                {
                    using (UnityWebRequest request = UnityWebRequest.Get(src))
                    {
                        yield return request.SendWebRequest();
                        result = request.downloadHandler.data;
                    }
                }
                else
                {
                    result = File.ReadAllBytes(src);
                }

                File.WriteAllBytes(dst, result);

                if (filename == htmlFileName)
                    htmlFilePath = dst;
            }

            _webViewObject.LoadURL("file://" + htmlFilePath.Replace(" ", "%20"));
        }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
    }
}
