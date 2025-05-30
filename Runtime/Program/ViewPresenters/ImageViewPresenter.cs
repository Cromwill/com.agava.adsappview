using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace AdsAppView.Program
{
    public class ImageViewPresenter : BaseViewPresenter
    {
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private Image _popupImage;

        public override bool Background => true;

        public override void Show(PopupData popupData)
        {
            Sprite popupSprite = FileUtils.LoadSprite(popupData.body);
            _popupImage.sprite = popupSprite;
            _aspectRatioFitter.aspectRatio = (float)popupSprite.texture.width / popupSprite.texture.height;
            LastPopupName = popupData.name;

            if (popupData.background != null)
                background.sprite = popupData.background;

            if (popupData.play_button != null)
                linkButtonImage.sprite = popupData.play_button;

            link = popupData.link;
            lastSpriteName = popupData.name;

            EnableCanvasGroup();
        }

        protected override IEnumerator Enabling()
        {
            _popupImage.enabled = false;

            float enablingTime = ViewPresenterConfigs.EnablingTime;
            float closingDelay = ViewPresenterConfigs.ClosingDelay;

            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(enablingTime);

            StartCoroutine(FadeIn.FadeInGraphic(background, enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeIn.FadeInGraphic(_popupImage, enablingTime));
            yield return waitForFadeIn;

            linkButton.gameObject.SetActive(true);

            StartCoroutine(FadeIn.FadeInGraphic(linkButtonImage, enablingTime));
            yield return waitForFadeIn;

            yield return new WaitForSecondsRealtime(closingDelay);
            closeButton.gameObject.SetActive(true);
        }
    }
}
