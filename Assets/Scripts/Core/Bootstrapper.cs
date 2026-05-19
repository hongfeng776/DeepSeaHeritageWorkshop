using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeManagers()
    {
        GameObject managersRoot = new GameObject("[GameManagers]");
        DontDestroyOnLoad(managersRoot);

        managersRoot.AddComponent<GameManager>();
        managersRoot.AddComponent<EventManager>();
        managersRoot.AddComponent<AudioManager>();
        managersRoot.AddComponent<SceneLoader>();
    }
}
