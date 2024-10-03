using System;
using AdsAppView.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace AdsAppView.Utility
{
    public class ViewPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _windowCanvasGrp;
        [SerializeField] private Image _image;
        [SerializeField] private Button _linkBtn;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private Button _closeBtn;

        private Links _links;
        private string _link;

        public bool Enabled { get; private set; } = false;

        public void Construct(Links links)
        {
            _links = links;
            _linkBtn.onClick.AddListener(OnLinkClicked);
            _closeBtn.onClick.AddListener(OnCloseClicked);
            DisableCanvasGroup(_windowCanvasGrp);
        }

        public void Show(SpriteData sprite)
        {
            _image.sprite = sprite.sprite;
            _aspectRatioFitter.aspectRatio = sprite.aspectRatio;
            _link = sprite.link;
            EnableCanvasGroup(_windowCanvasGrp);
        }

        private void OnLinkClicked()
        {
#if UNITY_EDITOR
            Debug.LogFormat($"Open link {_link}");
#endif

            if (string.IsNullOrEmpty(_link))
                return;

            Application.OpenURL(_link);
        }

        private void OnCloseClicked() => DisableCanvasGroup(_windowCanvasGrp);

        private void EnableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 1;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            Enabled = true;
        }

        private void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            Enabled = false;
        }
    }
}
