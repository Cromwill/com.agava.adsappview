using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AdsAppView.Program
{
    public class VideoViewPresenter : BaseViewPresenter
    {
        [SerializeField] private VideoPlayer _player;
        [SerializeField] private CanvasGroup _particlesGroup;

        private RawImage _videoPlayerImage;

        public override void Show(PopupData popupData)
        {
            _videoPlayerImage = _player.GetComponent<RawImage>();
            link = popupData.link;
            lastSpriteName = popupData.name;
            _player.url = popupData.path;
            EnableCanvasGroup();
        }

        protected override IEnumerator Enabling()
        {
            _particlesGroup.enabled = false;
            _particlesGroup.gameObject.SetActive(false);
            _particlesGroup.alpha = 0.0f;
            _videoPlayerImage.enabled = false;

            float enablingTime = ViewPresenterConfigs.EnablingTime;
            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(enablingTime);

            StartCoroutine(FadeIn.FadeInGraphic(background, enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            _player.Play();
            _player.loopPointReached += OnLoopPointReached;

            StartCoroutine(FadeIn.FadeInGraphic(_videoPlayerImage, enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeIn.FadeInCanvasGroup(_particlesGroup, enablingTime));
            yield return new WaitForSecondsRealtime(enablingTime);

            void OnLoopPointReached(VideoPlayer _)
            {
                closeButton.gameObject.SetActive(true);
            }

            linkButton.gameObject.SetActive(true);
            StartCoroutine(FadeIn.FadeInGraphic(linkButtonImage, enablingTime));
            yield return waitForFadeIn;
        }
    }
}
