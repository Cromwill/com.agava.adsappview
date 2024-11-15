using System.Collections.Generic;
using UnityEngine;

namespace AdsAppView.Program
{
    public class ViewPresenterFactory : MonoBehaviour
    {
        [SerializeField] private ImageViewPresenter _imageViewPresenter;
        [SerializeField] private VideoViewPresenter _videoViewPresenter;
        [SerializeField] private WebViewPresenter _webViewPresenter;
        [SerializeField] private AnimationViewPresenter _animationViewPresenter;

        private Dictionary<string, GameObject> _mapping;

        public IViewPresenter InstantiateViewPresenter(string type)
        {
            _mapping ??= new Dictionary<string, GameObject>()
            {
                { "image", _imageViewPresenter ?.gameObject},
                { "video", _videoViewPresenter?.gameObject},
                { "web", _webViewPresenter?.gameObject},
                { "animation", _animationViewPresenter?.gameObject },
                {"default", _imageViewPresenter?.gameObject},
            };

            if (_mapping.TryGetValue(type, out GameObject viewPresenter) == false || viewPresenter == null)
            {
                viewPresenter = _mapping["default"].gameObject;
            }

            return Instantiate(viewPresenter, transform).GetComponent<IViewPresenter>();
        }
    }
}
