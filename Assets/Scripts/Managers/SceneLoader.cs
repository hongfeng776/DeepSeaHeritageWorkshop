using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    public bool IsLoading { get; private set; }
    public float LoadProgress { get; private set; }

    [Header("Settings")]
    [SerializeField] private float transitionInDuration = 0.5f;
    [SerializeField] private float transitionOutDuration = 0.5f;
    [SerializeField] private bool useTransition = true;

    private AsyncOperation currentLoadOperation;
    private string targetSceneName;

    public void LoadScene(string sceneName, bool showLoadingScreen = true)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"Already loading a scene. Cannot load {sceneName}");
            return;
        }

        targetSceneName = sceneName;
        StartCoroutine(LoadSceneWithTransition(sceneName, showLoadingScreen));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName, bool showLoadingScreen)
    {
        IsLoading = true;
        LoadProgress = 0f;

        if (useTransition && SceneTransition.Instance != null)
        {
            yield return SceneTransition.Instance.TransitionIn(TransitionType.Fade, transitionInDuration);
        }

        EventManager.Instance.TriggerEvent(GameEventNames.OnSceneLoadStarted, sceneName);

        if (showLoadingScreen)
        {
            GameManager.Instance.SetGameState(GameState.Loading);
        }

        currentLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        currentLoadOperation.allowSceneActivation = false;

        while (!currentLoadOperation.isDone)
        {
            LoadProgress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
            EventManager.Instance.TriggerEvent(GameEventNames.OnSceneLoadProgress, LoadProgress);

            if (currentLoadOperation.progress >= 0.9f)
            {
                currentLoadOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        IsLoading = false;
        LoadProgress = 1f;

        EventManager.Instance.TriggerEvent(GameEventNames.OnSceneLoadCompleted, sceneName);

        if (showLoadingScreen)
        {
            GameManager.Instance.SetGameState(GameState.Playing);
        }

        if (useTransition && SceneTransition.Instance != null)
        {
            yield return SceneTransition.Instance.TransitionOut(TransitionType.Fade, transitionOutDuration);
        }
    }

    public void LoadSceneWithCustomTransition(string sceneName, TransitionType type, float duration, bool showLoadingScreen = true)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"Already loading a scene. Cannot load {sceneName}");
            return;
        }

        targetSceneName = sceneName;
        StartCoroutine(LoadSceneWithCustomTransitionCoroutine(sceneName, type, duration, showLoadingScreen));
    }

    private IEnumerator LoadSceneWithCustomTransitionCoroutine(string sceneName, TransitionType type, float duration, bool showLoadingScreen)
    {
        IsLoading = true;
        LoadProgress = 0f;

        if (useTransition && SceneTransition.Instance != null)
        {
            yield return SceneTransition.Instance.TransitionIn(type, duration);
        }

        EventManager.Instance.TriggerEvent(GameEventNames.OnSceneLoadStarted, sceneName);

        if (showLoadingScreen)
        {
            GameManager.Instance.SetGameState(GameState.Loading);
        }

        currentLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        currentLoadOperation.allowSceneActivation = false;

        while (!currentLoadOperation.isDone)
        {
            LoadProgress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
            EventManager.Instance.TriggerEvent(GameEventNames.OnSceneLoadProgress, LoadProgress);

            if (currentLoadOperation.progress >= 0.9f)
            {
                currentLoadOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        IsLoading = false;
        LoadProgress = 1f;

        EventManager.Instance.TriggerEvent(GameEventNames.OnSceneLoadCompleted, sceneName);

        if (showLoadingScreen)
        {
            GameManager.Instance.SetGameState(GameState.Playing);
        }

        if (useTransition && SceneTransition.Instance != null)
        {
            yield return SceneTransition.Instance.TransitionOut(type, duration);
        }
    }

    public void ReloadCurrentScene(bool showLoadingScreen = true)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene, showLoadingScreen);
    }

    public void ReturnToMainScene()
    {
        LoadScene("MainScene");
    }

    public void GoToCaveExploration()
    {
        LoadScene("CaveExplorationScene");
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public int GetCurrentSceneBuildIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public string GetTargetSceneName()
    {
        return targetSceneName;
    }
}

