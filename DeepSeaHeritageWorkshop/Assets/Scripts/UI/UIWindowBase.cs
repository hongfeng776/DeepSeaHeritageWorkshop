using UnityEngine;

public abstract class UIWindowBase : MonoBehaviour
{
    [Header("Window Settings")]
    [SerializeField] private UIWindowLayer windowLayer = UIWindowLayer.Normal;
    [SerializeField] private bool isUnique = true;
    [SerializeField] private bool closeOtherWindows = false;
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private bool usePool = false;

    private Canvas canvas;
    private RectTransform rectTransform;
    private object currentUserData;

    public UIWindowLayer WindowLayer => windowLayer;
    public bool IsUnique => isUnique;
    public bool CloseOtherWindows => closeOtherWindows;
    public bool UseAnimation => useAnimation;
    public bool UsePool => usePool;
    public bool IsOpen { get; protected set; }
    public string WindowName => GetType().Name;
    public object CurrentUserData => currentUserData;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
    }

    public virtual void Open(object userData = null)
    {
        if (IsOpen)
        {
            Refresh(userData);
            return;
        }

        gameObject.SetActive(true);
        IsOpen = true;
        currentUserData = userData;

        OnBeforeOpen(userData);
        OnOpen(userData);

        if (useAnimation)
        {
            PlayOpenAnimation(OnAfterOpen);
        }
        else
        {
            OnAfterOpen();
        }
    }

    public virtual void Close(bool immediate = false)
    {
        if (!IsOpen)
            return;

        OnBeforeClose();

        IsOpen = false;

        if (useAnimation && !immediate)
        {
            PlayCloseAnimation(() =>
            {
                OnClose();
                OnAfterClose();
                gameObject.SetActive(false);
            });
        }
        else
        {
            OnClose();
            OnAfterClose();
            gameObject.SetActive(false);
        }
    }

    public virtual void Refresh(object userData = null)
    {
        if (userData != null)
        {
            currentUserData = userData;
        }
        OnRefresh(currentUserData);
    }

    protected virtual void OnBeforeOpen(object userData = null)
    {
    }

    protected virtual void OnOpen(object userData = null)
    {
    }

    protected virtual void OnAfterOpen()
    {
    }

    protected virtual void OnBeforeClose()
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnAfterClose()
    {
    }

    protected virtual void OnRefresh(object userData = null)
    {
    }

    protected virtual void PlayOpenAnimation(System.Action onComplete = null)
    {
        onComplete?.Invoke();
    }

    protected virtual void PlayCloseAnimation(System.Action onComplete = null)
    {
        onComplete?.Invoke();
    }

    public virtual void OnReuse()
    {
    }

    public virtual void OnRecycle()
    {
    }

    public void SetSortingOrder(int order)
    {
        if (canvas != null)
        {
            canvas.sortingOrder = order;
        }
    }

    public int GetSortingOrder()
    {
        return canvas != null ? canvas.sortingOrder : 0;
    }

    public void SetLayer(UIWindowLayer newLayer)
    {
        windowLayer = newLayer;
    }
}
