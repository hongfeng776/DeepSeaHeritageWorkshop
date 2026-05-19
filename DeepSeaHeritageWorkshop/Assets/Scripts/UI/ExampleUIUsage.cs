using UnityEngine;

public class ExampleUIUsage : MonoBehaviour
{
    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        UIManager.Instance.PreloadWindow<ConfirmationWindow>(5);
        Debug.Log($"Preloaded ConfirmationWindow, pool size: {UIManager.Instance.GetPoolSize<ConfirmationWindow>()}");
    }

    private void OpenMainMenu()
    {
        UIManager.Instance.OpenWindow<MainMenuWindow>();
    }

    private void OpenSettings()
    {
        UIManager.Instance.OpenWindow<SettingsWindow>();
    }

    private void OpenConfirmation()
    {
        var data = new ConfirmationData
        {
            Title = "提示",
            Message = "确定要执行此操作吗？",
            ConfirmText = "确定",
            CancelText = "取消",
            OnConfirm = () => Debug.Log("用户点击了确认"),
            OnCancel = () => Debug.Log("用户点击了取消")
        };

        UIManager.Instance.OpenWindow<ConfirmationWindow>(data);
    }

    private void RefreshOpenedWindow()
    {
        if (UIManager.Instance.IsWindowOpen<ConfirmationWindow>())
        {
            var newData = new ConfirmationData
            {
                Title = "更新后的标题",
                Message = "这是刷新后的消息内容",
                ConfirmText = "好的",
                CancelText = "关闭"
            };

            var window = UIManager.Instance.GetWindow<ConfirmationWindow>();
            window.Refresh(newData);
        }
    }

    private void CloseTopWindow()
    {
        UIManager.Instance.CloseTopWindow();
    }

    private void CloseAllWindows()
    {
        UIManager.Instance.CloseAllWindows();
    }

    private void BringWindowToFront()
    {
        var window = UIManager.Instance.GetWindow<ConfirmationWindow>();
        if (window != null)
        {
            UIManager.Instance.BringToFront(window);
        }
    }

    private void GetAllWindowsOfType()
    {
        var allConfirmWindows = UIManager.Instance.GetAllWindows<ConfirmationWindow>();
        Debug.Log($"当前打开了 {allConfirmWindows.Count} 个 ConfirmationWindow");
    }

    private void ClearPool()
    {
        UIManager.Instance.ClearPool<ConfirmationWindow>();
        Debug.Log($"Cleared ConfirmationWindow pool, current size: {UIManager.Instance.GetPoolSize<ConfirmationWindow>()}");
    }

    private void ClearAllPools()
    {
        UIManager.Instance.ClearAllPools();
        Debug.Log("Cleared all pools");
    }
}
