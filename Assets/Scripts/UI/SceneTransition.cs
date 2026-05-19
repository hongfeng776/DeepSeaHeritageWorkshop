using System.Collections;
using UnityEngine;

public enum TransitionType
{
    Fade,
    CircleWipe,
    HorizontalWipe,
    VerticalWipe,
    CrossFade
}

public class SceneTransition : MonoSingleton<SceneTransition>
{
    [Header("Transition Settings")]
    [SerializeField] private TransitionType defaultTransition = TransitionType.Fade;
    [SerializeField] private float defaultDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Components")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private RectTransform transitionRect;
    [SerializeField] private Material transitionMaterial;

    private Coroutine currentTransition;
    private int _ProgressID = -1;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeComponents();
        _ProgressID = Shader.PropertyToID("_Progress");
    }

    private void InitializeComponents()
    {
        if (transitionRect == null)
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.transform.SetParent(transform);

            GameObject rectObj = new GameObject("TransitionRect");
            transitionRect = rectObj.AddComponent<RectTransform>();
            transitionRect.SetParent(canvasObj.transform);
            transitionRect.anchorMin = Vector2.zero;
            transitionRect.anchorMax = Vector2.one;
            transitionRect.sizeDelta = Vector2.zero;
            transitionRect.anchoredPosition = Vector2.zero;

            UnityEngine.UI.Image image = rectObj.AddComponent<UnityEngine.UI.Image>();
            image.color = fadeColor;
            image.material = transitionMaterial;

            fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void TransitionIn(System.Action onComplete = null)
    {
        TransitionIn(defaultTransition, defaultDuration, onComplete);
    }

    public void TransitionIn(TransitionType type, float duration, System.Action onComplete = null)
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        currentTransition = StartCoroutine(TransitionInCoroutine(type, duration, onComplete));
    }

    public void TransitionOut(System.Action onComplete = null)
    {
        TransitionOut(defaultTransition, defaultDuration, onComplete);
    }

    public void TransitionOut(TransitionType type, float duration, System.Action onComplete = null)
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        currentTransition = StartCoroutine(TransitionOutCoroutine(type, duration, onComplete));
    }

    private IEnumerator TransitionInCoroutine(TransitionType type, float duration, System.Action onComplete)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        SetTransitionType(type);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            if (type == TransitionType.Fade)
            {
                fadeCanvasGroup.alpha = progress;
            }
            else
            {
                transitionMaterial.SetFloat(_ProgressID, progress);
                fadeCanvasGroup.alpha = 1f;
            }

            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        transitionMaterial.SetFloat(_ProgressID, 1f);
        currentTransition = null;
        onComplete?.Invoke();
    }

    private IEnumerator TransitionOutCoroutine(TransitionType type, float duration, System.Action onComplete)
    {
        SetTransitionType(type);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = 1f - (elapsedTime / duration);

            if (type == TransitionType.Fade)
            {
                fadeCanvasGroup.alpha = progress;
            }
            else
            {
                transitionMaterial.SetFloat(_ProgressID, progress);
            }

            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        transitionMaterial.SetFloat(_ProgressID, 0f);
        fadeCanvasGroup.blocksRaycasts = false;
        currentTransition = null;
        onComplete?.Invoke();
    }

    private void SetTransitionType(TransitionType type)
    {
        if (transitionMaterial == null)
            return;
    }

    public void SetColor(Color color)
    {
        fadeColor = color;
        var image = transitionRect.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = color;
        }
    }

    public bool IsTransitioning => currentTransition != null;
}
