using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Text progressText;
    [SerializeField] private Text tipText;
    [SerializeField] private GameObject loadingContent;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float minimumDisplayTime = 1f;

    [Header("Tips")]
    [SerializeField] private string[] loadingTips = new string[]
    {
        "探索洞穴时注意收集资源...",
        "升级工坊可以提高效率...",
        "修复文物可以解锁新区域...",
        "合理分配资源很重要...",
        "定期查看任务获取奖励..."
    };

    private float loadStartTime;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        DontDestroyOnLoad(gameObject);
        HideImmediate();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<string>(GameEventNames.OnSceneLoadStarted, OnSceneLoadStarted);
        EventManager.Instance.AddListener<float>(GameEventNames.OnSceneLoadProgress, OnSceneLoadProgress);
        EventManager.Instance.AddListener<string>(GameEventNames.OnSceneLoadCompleted, OnSceneLoadCompleted);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<string>(GameEventNames.OnSceneLoadStarted, OnSceneLoadStarted);
        EventManager.Instance.RemoveListener<float>(GameEventNames.OnSceneLoadProgress, OnSceneLoadProgress);
        EventManager.Instance.RemoveListener<string>(GameEventNames.OnSceneLoadCompleted, OnSceneLoadCompleted);
    }

    private void OnSceneLoadStarted(string sceneName)
    {
        loadStartTime = Time.time;
        ShowRandomTip();
        Show();
    }

    private void OnSceneLoadProgress(float progress)
    {
        UpdateProgress(progress);
    }

    private void OnSceneLoadCompleted(string sceneName)
    {
        StartCoroutine(WaitAndHide());
    }

    private void ShowRandomTip()
    {
        if (tipText != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }
    }

    public void Show()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        loadingContent?.SetActive(true);
        UpdateProgress(0f);
        currentFadeCoroutine = StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(FadeOut());
    }

    public void HideImmediate()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        loadingContent?.SetActive(false);
    }

    public void UpdateProgress(float progress)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }
        if (progressText != null)
        {
            progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
        }
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
            yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        loadingContent?.SetActive(false);
        currentFadeCoroutine = null;
    }

    private IEnumerator WaitAndHide()
    {
        float elapsedTime = Time.time - loadStartTime;
        float remainingTime = minimumDisplayTime - elapsedTime;

        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        Hide();
    }
}
