using UnityEngine;
using UnityEngine.UI;

public class MainMenuWindow : UIWindowBase
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    protected override void Awake()
    {
        base.Awake();
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        startGameButton?.onClick.AddListener(OnStartGameClicked);
        settingsButton?.onClick.AddListener(OnSettingsClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
    }

    private void OnStartGameClicked()
    {
        UIManager.Instance.CloseWindow<MainMenuWindow>();
        GameManager.Instance.SetGameState(GameState.Playing);
    }

    private void OnSettingsClicked()
    {
        UIManager.Instance.OpenWindow<SettingsWindow>();
    }

    private void OnQuitClicked()
    {
        UIManager.Instance.OpenWindow<ConfirmationWindow>(new ConfirmationData
        {
            Title = "退出游戏",
            Message = "确定要退出游戏吗？",
            OnConfirm = () => GameManager.Instance.QuitGame(),
            OnCancel = null
        });
    }

    protected override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);
    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    private void OnDestroy()
    {
        startGameButton?.onClick.RemoveAllListeners();
        settingsButton?.onClick.RemoveAllListeners();
        quitButton?.onClick.RemoveAllListeners();
    }
}
