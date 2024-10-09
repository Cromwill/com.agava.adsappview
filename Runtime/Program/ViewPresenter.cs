using System;
using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace AdsAppView.Program
{
    public class ViewPresenter : MonoBehaviour
    {
        private const float Diff = 0.5f;

        [SerializeField] private CanvasGroup _windowCanvasGrp;
        [SerializeField] private Button _linkButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private Image _popupImage;
        [SerializeField] private Image _background;

        private Image _linkButtonImage;

        private float _closingDelay = 0;
        private float _enablingTime = 10;
        private string _link;
        private string _lastSpriteName;
        private int _count = 0;

        private Coroutine _enablingCoroutine;

        public bool Enable { get; private set; } = false;

        public Action Enabled;
        public Action Disabled;

        private void Awake()
        {
            DisableCanvasGroup(_windowCanvasGrp);
            _linkButtonImage = _linkButton.GetComponent<Image>();
        }

        private void OnEnable()
        {
            _linkButton.onClick.AddListener(OnLinkClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDisable()
        {
            _linkButton.onClick.RemoveListener(OnLinkClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        public void Initialize(float enablingTime, float closingDelay)
        {
            _enablingTime = enablingTime;
            _closingDelay = closingDelay;
        }

        public void Show(SpriteData sprite)
        {
            _count++;
            _popupImage.sprite = sprite.sprite;
            _aspectRatioFitter.aspectRatio = sprite.aspectRatio;
            _link = sprite.link;
            _lastSpriteName = sprite.name;

            Stop(_enablingCoroutine);
            _enablingCoroutine = StartCoroutine(EnableCanvasGroup(_windowCanvasGrp));
        }

        private void OnLinkClicked()
        {
#if UNITY_EDITOR
            Debug.LogFormat($"#ViewPresenter# Open link {_link}");
#endif

            if (string.IsNullOrEmpty(_link))
                return;

            AnalyticsService.SendPopupRedirectClick(_lastSpriteName, _count);
            Application.OpenURL(_link);
        }

        private void OnCloseClicked()
        {
            AnalyticsService.SendPopupClosed();
            DisableCanvasGroup(_windowCanvasGrp);
        }

        private IEnumerator EnableCanvasGroup(CanvasGroup canvas)
        {
            _closeButton.gameObject.SetActive(false);
            _linkButton.gameObject.SetActive(false);

            _popupImage.enabled = false;
            _background.enabled = false;
            _linkButtonImage.enabled = false;

            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            canvas.alpha = 1;
            Enable = true;
            Enabled?.Invoke();

            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(_enablingTime);

            StartCoroutine(FadeInImage(_background, _enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeInImage(_popupImage, _enablingTime));
            yield return waitForFadeIn;

            _linkButton.gameObject.SetActive(true);

            StartCoroutine(FadeInImage(_linkButtonImage, _enablingTime));
            yield return waitForFadeIn;

            yield return new WaitForSecondsRealtime(_closingDelay);
            _closeButton.gameObject.SetActive(true);
        }

        private void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            Enable = false;
            Disabled?.Invoke();
            Stop(_enablingCoroutine);
        }

        private IEnumerator FadeInImage(Image image, float time)
        {
            image.enabled = true;

            image.gameObject.SetActive(true);
            Color color = image.color;

            if (time > 0)
            {
                color.a = 0;
                float elapsedTime = 0;

                while (elapsedTime < _enablingTime)
                {
                    color.a = Mathf.Lerp(0, 1, elapsedTime / _enablingTime);
                    image.color = color;
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }

            color.a = 1;
            image.color = color;
        }

        private void Stop(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    }
}
