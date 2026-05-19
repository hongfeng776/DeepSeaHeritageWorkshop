using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    private GameState currentGameState = GameState.Boot;

    public GameState CurrentGameState => currentGameState;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            InitializeGame();
        }
    }

    private void Start()
    {
        SetGameState(GameState.MainMenu);
    }

    private void InitializeGame()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    public void SetGameState(GameState newState)
    {
        if (currentGameState == newState)
            return;

        GameState previousState = currentGameState;
        currentGameState = newState;

        OnStateChanged(previousState, newState);
        EventManager.Instance.TriggerEvent(GameEventNames.OnGameStateChanged, currentGameState);
    }

    private void OnStateChanged(GameState previousState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.MainMenu:
            case GameState.Loading:
            case GameState.GameOver:
                Time.timeScale = 1f;
                break;
        }
    }

    public void TogglePause()
    {
        if (currentGameState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
        else if (currentGameState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
