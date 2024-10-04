using System;
using UnityEngine;

namespace AdsAppView.Program
{
    public class GamePause : MonoBehaviour
    {
        [SerializeField] private ViewPresenter _viewPresenter;

        private static GamePause s_instance;

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

        private void OnEnable()
        {
            s_instance._viewPresenter.Enabled += OnAdsEnabled;
            s_instance._viewPresenter.Disabled += OnAdsDisabled;
        }

        private void OnDisable()
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
