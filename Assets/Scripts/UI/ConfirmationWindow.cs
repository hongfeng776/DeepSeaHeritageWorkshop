using UnityEngine;
using UnityEngine.UI;

public class ConfirmationData
{
    public string Title;
    public string Message;
    public string ConfirmText;
    public string CancelText;
    public System.Action OnConfirm;
    public System.Action OnCancel;
}

public class ConfirmationWindow : UIWindowBase
{
    [Header("Confirmation Elements")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Text confirmButtonText;
    [SerializeField] private Text cancelButtonText;

    private ConfirmationData currentData;

    protected override void Awake()
    {
        base.Awake();
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        confirmButton?.onClick.AddListener(OnConfirmClicked);
        cancelButton?.onClick.AddListener(OnCancelClicked);
    }

    protected override void OnBeforeOpen(object userData = null)
    {
        base.OnBeforeOpen(userData);
        Debug.Log($"[ConfirmationWindow] OnBeforeOpen - 窗口即将打开");
    }

    protected override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        if (userData is ConfirmationData data)
        {
            currentData = data;
            UpdateUI(data);
        }

        Debug.Log($"[ConfirmationWindow] OnOpen - 窗口已打开");
    }

    protected override void OnAfterOpen()
    {
        base.OnAfterOpen();
        Debug.Log($"[ConfirmationWindow] OnAfterOpen - 窗口打开动画播放完成");
    }

    protected override void OnRefresh(object userData = null)
    {
        base.OnRefresh(userData);

        if (userData is ConfirmationData data)
        {
            currentData = data;
            UpdateUI(data);
        }

        Debug.Log($"[ConfirmationWindow] OnRefresh - 窗口已刷新");
    }

    protected override void OnBeforeClose()
    {
        base.OnBeforeClose();
        Debug.Log($"[ConfirmationWindow] OnBeforeClose - 窗口即将关闭");
    }

    protected override void OnClose()
    {
        base.OnClose();
        Debug.Log($"[ConfirmationWindow] OnClose - 窗口正在关闭");
    }

    protected override void OnAfterClose()
    {
        base.OnAfterClose();
        Debug.Log($"[ConfirmationWindow] OnAfterClose - 窗口关闭完成");
    }

    public override void OnReuse()
    {
        base.OnReuse();
        Debug.Log($"[ConfirmationWindow] OnReuse - 窗口从对象池中复用");
    }

    public override void OnRecycle()
    {
        base.OnRecycle();
        currentData = null;
        Debug.Log($"[ConfirmationWindow] OnRecycle - 窗口回收至对象池");
    }

    private void UpdateUI(ConfirmationData data)
    {
        if (titleText != null)
        {
            titleText.text = data.Title ?? "确认";
        }
        if (messageText != null)
        {
            messageText.text = data.Message ?? "确定执行此操作？";
        }
        if (confirmButtonText != null)
        {
            confirmButtonText.text = data.ConfirmText ?? "确认";
        }
        if (cancelButtonText != null)
        {
            cancelButtonText.text = data.CancelText ?? "取消";
        }
    }

    private void OnConfirmClicked()
    {
        currentData?.OnConfirm?.Invoke();
        UIManager.Instance.CloseWindow(this);
    }

    private void OnCancelClicked()
    {
        currentData?.OnCancel?.Invoke();
        UIManager.Instance.CloseWindow(this);
    }

    private void OnDestroy()
    {
        confirmButton?.onClick.RemoveAllListeners();
        cancelButton?.onClick.RemoveAllListeners();
    }
}
