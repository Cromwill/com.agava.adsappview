using UnityEngine;
using UnityEngine.UI;

namespace AdsAppView.Program
{
    public class LoadingBarPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform _progressBar;
        [SerializeField] private Image _fill;

        private int _max;
        private int _current = 0;

        private void Awake()
        {
            _fill.fillAmount = 0;
        }

        public void SetActive(bool enable) => _progressBar.gameObject.SetActive(enable);

        public void UpdateProgress(int current, int max)
        {
            float value = Mathf.InverseLerp(max, 0, current);
            _fill.fillAmount = Mathf.Lerp(1, 0, value);
        }

        public void SetMax(int max) => _max = max;

        public void UpdateAdditiveProgress()
        {
            if (_max <= 0)
                return;

            _current++;
            float value = Mathf.InverseLerp(_max, 0, _current);
            _fill.fillAmount = Mathf.Lerp(1, 0, value);
        }
    }
}
