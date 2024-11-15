using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AdsAppView.Utility
{
    public static class FadeIn
    {
        public static IEnumerator FadeInGraphic(Graphic graphicObject, float time)
        {
            graphicObject.enabled = true;
            graphicObject.gameObject.SetActive(true);
            Color color = graphicObject.color;

            if (time > 0)
            {
                color.a = 0;
                float elapsedTime = 0;

                while (elapsedTime < time)
                {
                    color.a = Mathf.Lerp(0, 1, elapsedTime / time);
                    graphicObject.color = color;
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }

            color.a = 1;
            graphicObject.color = color;
        }

        public static IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float time)
        {
            canvasGroup.gameObject.SetActive(true);

            if (time > 0)
            {
                canvasGroup.alpha = 0.0f;
                float elapsedTime = 0.0f;

                while (elapsedTime < time)
                {
                    canvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / time);
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }

            canvasGroup.alpha = 1.0f;
        }
    }
}
