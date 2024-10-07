using System;
using System.Collections;
using AdsAppView.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace AdsAppView.Program
{
    public class ViewPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _windowCanvasGrp;
        [SerializeField] private Image _image;
        [SerializeField] private Button _linkBtn;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private Button _closeBtn;

        private float _closingDelay = 0;
        private string _link;

        private Coroutine _coroutine;

        public bool Enable { get; private set; } = false;

        public Action Enabled;
        public Action Disabled;

        private void Awake()
        {
            DisableCanvasGroup(_windowCanvasGrp);
        }

        private void OnEnable()
        {
            _linkBtn.onClick.AddListener(OnLinkClicked);
            _closeBtn.onClick.AddListener(OnCloseClicked);
        }

        private void OnDisable()
        {
            _linkBtn.onClick.RemoveListener(OnLinkClicked);
            _closeBtn.onClick.RemoveListener(OnCloseClicked);
        }

        public void Initialize(float closingDelay)
        {
            _closingDelay = closingDelay;
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
            Enable = true;
            Enabled?.Invoke();
            StopCoroutine();
            _coroutine = StartCoroutine(ShowCloseButtonWithDelay(_closingDelay));
        }

        private void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            Enable = false;
            Disabled?.Invoke();
            StopCoroutine();
        }

        private IEnumerator ShowCloseButtonWithDelay(float delay)
        {
            if (delay > 0)
            {
                _closeBtn.gameObject.SetActive(false);

                WaitForSeconds waitForSeconds = new WaitForSeconds(delay);
                yield return waitForSeconds;

                _closeBtn.gameObject.SetActive(true);
            }

            _coroutine = null;
        }

        private void StopCoroutine()
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }
    }
}
