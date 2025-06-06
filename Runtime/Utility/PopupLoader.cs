using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AdsAppView.Program;
using UnityEngine.SceneManagement;

namespace AdsAppView.Utility
{
    public class PopupLoader : MonoBehaviour
    {
        [SerializeField] private Boot _boot;
        [SerializeField] private Image _loadingImage;
        [SerializeField] private string _startSceneName;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => _boot.Constructed);

            SceneManager.LoadSceneAsync(_startSceneName);
        }

        private void Update()
        {
            _loadingImage.transform.localEulerAngles += new Vector3(0, 0, 2f);
        }
    }
}
