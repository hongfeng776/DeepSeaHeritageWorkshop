using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    public bool IsLoading { get; private set; }
    public float LoadProgress { get; private set; }

    private AsyncOperation currentLoadOperation;

    public void LoadScene(string sceneName, bool showLoadingScreen = true)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"Already loading a scene. Cannot load {sceneName}");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName, showLoadingScreen));
    }

    private IEnumerator LoadSceneAsync(string sceneName, bool showLoadingScreen)
    {
        IsLoading = true;
        LoadProgress = 0f;

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
    }

    public void ReloadCurrentScene(bool showLoadingScreen = true)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene, showLoadingScreen);
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public int GetCurrentSceneBuildIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
}
