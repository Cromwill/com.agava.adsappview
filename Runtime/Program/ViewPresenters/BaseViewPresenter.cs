using System;
using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Program;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseViewPresenter : MonoBehaviour, IViewPresenter
{
    protected const float Diff = 0.5f;

    [SerializeField] private CanvasGroup _windowCanvasGrp;

    [field: SerializeField] protected Button linkButton;
    [field: SerializeField] protected Button closeButton;
    [field: SerializeField] protected Image background;

    private int _count = 0;
    private Coroutine _enablingCoroutine;

    protected Image linkButtonImage;
    protected string link;
    protected string lastSpriteName;

    public bool Enable { get; private set; } = false;

    public event Action Enabled;
    public event Action Disabled;

    private void Awake()
    {
        DisableCanvasGroup(_windowCanvasGrp);
        linkButtonImage = linkButton.GetComponent<Image>();
    }

    private void OnEnable()
    {
        linkButton.onClick.AddListener(OnLinkClicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDisable()
    {
        linkButton.onClick.RemoveListener(OnLinkClicked);
        closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    public abstract void Show(PopupData popupData);

    protected abstract IEnumerator Enabling();

    protected void EnableCanvasGroup()
    {
        _count++;

        closeButton.gameObject.SetActive(false);
        linkButton.gameObject.SetActive(false);

        background.enabled = false;
        linkButtonImage.enabled = false;

        _windowCanvasGrp.interactable = true;
        _windowCanvasGrp.blocksRaycasts = true;
        _windowCanvasGrp.alpha = 1;
        Enable = true;
        Enabled?.Invoke();

        Stop(_enablingCoroutine);
        _enablingCoroutine = StartCoroutine(Enabling());
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

    private void OnLinkClicked()
    {
#if UNITY_EDITOR
        Debug.LogFormat($"#ViewPresenter# Open link {link}");
#endif

        if (string.IsNullOrEmpty(link))
            return;

        AnalyticsService.SendPopupRedirectClick(lastSpriteName, _count);
        Application.OpenURL(link);
    }

    private void OnCloseClicked()
    {
        AnalyticsService.SendPopupClosed();
        DisableCanvasGroup(_windowCanvasGrp);
    }

    private void Stop(Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
}
