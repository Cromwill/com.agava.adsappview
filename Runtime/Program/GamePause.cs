using System;
using UnityEngine;

namespace AdsAppView.Program
{
    public class GamePause : MonoBehaviour
    {
        private static GamePause s_instance;

        private IViewPresenter _viewPresenter;

        public static Action GamePaused;
        public static Action GameUnpaused;

        private void Awake()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(IViewPresenter viewPresenter)
        {
            _viewPresenter = viewPresenter;
            s_instance._viewPresenter.Enabled += OnAdsEnabled;
            s_instance._viewPresenter.Disabled += OnAdsDisabled;
        }

        private void OnDestroy()
        {
            s_instance._viewPresenter.Enabled -= OnAdsEnabled;
            s_instance._viewPresenter.Disabled -= OnAdsDisabled;
        }

        private void OnAdsEnabled()
        {
            GamePaused?.Invoke();
        }

        private void OnAdsDisabled()
        {
            GameUnpaused?.Invoke();
        }
    }
}
