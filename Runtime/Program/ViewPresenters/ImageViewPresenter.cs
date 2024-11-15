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

        public override void Show(PopupData popupData)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(popupData.bytes);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            _popupImage.sprite = sprite;
            _aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
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
