using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    [Header("Pool Settings")]
    [SerializeField] private int defaultPoolSize = 3;
    [SerializeField] private bool enablePool = true;

    private Transform uiRoot;
    private Transform poolRoot;

    private Dictionary<Type, UIWindowBase> windowPrefabCache = new Dictionary<Type, UIWindowBase>();
    private Dictionary<Type, Queue<UIWindowBase>> windowPool = new Dictionary<Type, Queue<UIWindowBase>>();
    private Dictionary<Type, List<UIWindowBase>> activeWindows = new Dictionary<Type, List<UIWindowBase>>();
    private Stack<UIWindowBase> windowStack = new Stack<UIWindowBase>();
    private Dictionary<UIWindowLayer, int> layerSortingOrders = new Dictionary<UIWindowLayer, int>();

    public Transform UIRoot => uiRoot;
    public Transform PoolRoot => poolRoot;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            InitializeRoots();
            InitializeLayerSortingOrders();
        }
    }

    private void InitializeRoots()
    {
        if (uiRoot == null)
        {
            GameObject uiRootObj = new GameObject("UIRoot");
            uiRootObj.transform.SetParent(transform);
            uiRoot = uiRootObj.transform;

            Canvas canvas = uiRootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            uiRootObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            uiRootObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        if (poolRoot == null)
        {
            GameObject poolRootObj = new GameObject("PoolRoot");
            poolRootObj.transform.SetParent(transform);
            poolRootObj.SetActive(false);
            poolRoot = poolRootObj.transform;
        }
    }

    private void InitializeLayerSortingOrders()
    {
        foreach (UIWindowLayer layer in Enum.GetValues(typeof(UIWindowLayer)))
        {
            layerSortingOrders[layer] = (int)layer;
        }
    }

    public void PreloadWindow<T>(int count = -1) where T : UIWindowBase
    {
        if (!enablePool)
            return;

        Type windowType = typeof(T);
        T prefab = GetWindowPrefab<T>();
        if (prefab == null)
        {
            Debug.LogError($"Cannot preload window {windowType.Name}: prefab not found");
            return;
        }

        if (!prefab.UsePool)
        {
            Debug.LogWarning($"Window {windowType.Name} is not marked for pooling");
            return;
        }

        if (!windowPool.ContainsKey(windowType))
        {
            windowPool[windowType] = new Queue<UIWindowBase>();
        }

        int preloadCount = count <= 0 ? defaultPoolSize : count;
        int currentCount = windowPool[windowType].Count;

        for (int i = currentCount; i < preloadCount; i++)
        {
            UIWindowBase window = CreateWindowInstance(prefab);
            RecycleWindow(window);
        }

        Debug.Log($"Preloaded {preloadCount - currentCount} instances of {windowType.Name}");
    }

    public T OpenWindow<T>(object userData = null) where T : UIWindowBase
    {
        Type windowType = typeof(T);
        T windowPrefab = GetWindowPrefab<T>();

        if (windowPrefab == null)
        {
            Debug.LogError($"Window prefab not found for type: {windowType.Name}");
            return null;
        }

        if (windowPrefab.IsUnique)
        {
            T existingUniqueWindow = GetUniqueWindow<T>();
            if (existingUniqueWindow != null)
            {
                existingUniqueWindow.Open(userData);
                BringToFront(existingUniqueWindow);
                return existingUniqueWindow;
            }
        }

        T newWindow = GetOrCreateWindow(windowPrefab);
        newWindow.Open(userData);

        if (!activeWindows.ContainsKey(windowType))
        {
            activeWindows[windowType] = new List<UIWindowBase>();
        }
        activeWindows[windowType].Add(newWindow);
        windowStack.Push(newWindow);

        if (newWindow.CloseOtherWindows)
        {
            CloseAllWindowsExcept(newWindow);
        }

        return newWindow;
    }

    private T GetOrCreateWindow<T>(T prefab) where T : UIWindowBase
    {
        Type windowType = typeof(T);

        if (enablePool && prefab.UsePool)
        {
            if (windowPool.TryGetValue(windowType, out Queue<UIWindowBase> pool) && pool.Count > 0)
            {
                UIWindowBase pooledWindow = pool.Dequeue();
                pooledWindow.transform.SetParent(uiRoot);
                pooledWindow.OnReuse();
                return pooledWindow as T;
            }
        }

        return CreateWindowInstance(prefab);
    }

    private T CreateWindowInstance<T>(T prefab) where T : UIWindowBase
    {
        T window = Instantiate(prefab, uiRoot);
        window.name = $"{prefab.WindowName}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        int sortingOrder = GetNextSortingOrder(window.WindowLayer);
        window.SetSortingOrder(sortingOrder);

        return window;
    }

    public void CloseWindow<T>(bool immediate = false) where T : UIWindowBase
    {
        Type windowType = typeof(T);
        if (activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows) && windows.Count > 0)
        {
            UIWindowBase window = windows[windows.Count - 1];
            CloseWindow(window, immediate);
        }
    }

    public void CloseWindow(UIWindowBase window, bool immediate = false)
    {
        if (window == null || !window.IsOpen)
            return;

        window.Close(immediate);

        Type windowType = window.GetType();
        if (activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows))
        {
            windows.Remove(window);
        }

        RemoveFromStack(window);

        if (enablePool && window.UsePool)
        {
            RecycleWindow(window);
        }
        else
        {
            Destroy(window.gameObject);
        }
    }

    private void RecycleWindow(UIWindowBase window)
    {
        Type windowType = window.GetType();

        if (!windowPool.ContainsKey(windowType))
        {
            windowPool[windowType] = new Queue<UIWindowBase>();
        }

        window.OnRecycle();
        window.transform.SetParent(poolRoot);
        windowPool[windowType].Enqueue(window);
    }

    public void CloseTopWindow(bool immediate = false)
    {
        if (windowStack.Count > 0)
        {
            UIWindowBase topWindow = windowStack.Pop();
            CloseWindow(topWindow, immediate);
        }
    }

    public void CloseAllWindows(bool immediate = false)
    {
        List<UIWindowBase> allWindows = new List<UIWindowBase>();
        foreach (var kvp in activeWindows)
        {
            allWindows.AddRange(kvp.Value);
        }

        foreach (UIWindowBase window in allWindows)
        {
            window.Close(immediate);

            if (enablePool && window.UsePool)
            {
                RecycleWindow(window);
            }
            else
            {
                Destroy(window.gameObject);
            }
        }

        activeWindows.Clear();
        windowStack.Clear();
        ResetLayerSortingOrders();
    }

    public void CloseAllWindowsExcept(UIWindowBase exceptWindow, bool immediate = false)
    {
        List<UIWindowBase> windowsToClose = new List<UIWindowBase>();
        foreach (var kvp in activeWindows)
        {
            foreach (UIWindowBase window in kvp.Value)
            {
                if (window != exceptWindow)
                {
                    windowsToClose.Add(window);
                }
            }
        }

        foreach (UIWindowBase window in windowsToClose)
        {
            window.Close(immediate);

            Type windowType = window.GetType();
            if (activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows))
            {
                windows.Remove(window);
            }

            RemoveFromStack(window);

            if (enablePool && window.UsePool)
            {
                RecycleWindow(window);
            }
            else
            {
                Destroy(window.gameObject);
            }
        }
    }

    public T GetWindow<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        if (activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows) && windows.Count > 0)
        {
            return windows[windows.Count - 1] as T;
        }
        return null;
    }

    public List<T> GetAllWindows<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        List<T> result = new List<T>();

        if (activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows))
        {
            foreach (UIWindowBase window in windows)
            {
                result.Add(window as T);
            }
        }

        return result;
    }

    private T GetUniqueWindow<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        if (activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows) && windows.Count > 0)
        {
            return windows[0] as T;
        }
        return null;
    }

    public bool IsWindowOpen<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        return activeWindows.TryGetValue(windowType, out List<UIWindowBase> windows) && windows.Count > 0;
    }

    public void BringToFront(UIWindowBase window)
    {
        if (window == null)
            return;

        int sortingOrder = GetNextSortingOrder(window.WindowLayer);
        window.SetSortingOrder(sortingOrder);

        RemoveFromStack(window);
        windowStack.Push(window);
    }

    public void SendToBack(UIWindowBase window)
    {
        if (window == null)
            return;

        int sortingOrder = (int)window.WindowLayer;
        window.SetSortingOrder(sortingOrder);
    }

    private T GetWindowPrefab<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        if (!windowPrefabCache.TryGetValue(windowType, out UIWindowBase prefab))
        {
            string prefabPath = $"UI/{windowType.Name}";
            prefab = Resources.Load<T>(prefabPath);
            if (prefab != null)
            {
                windowPrefabCache.Add(windowType, prefab);
            }
        }
        return prefab as T;
    }

    private int GetNextSortingOrder(UIWindowLayer layer)
    {
        if (!layerSortingOrders.ContainsKey(layer))
        {
            layerSortingOrders[layer] = (int)layer;
        }

        int nextOrder = layerSortingOrders[layer];
        layerSortingOrders[layer] = nextOrder + 1;
        return nextOrder;
    }

    private void RemoveFromStack(UIWindowBase window)
    {
        List<UIWindowBase> tempList = new List<UIWindowBase>(windowStack);
        tempList.Remove(window);
        windowStack.Clear();
        foreach (UIWindowBase w in tempList)
        {
            windowStack.Push(w);
        }
    }

    private void ResetLayerSortingOrders()
    {
        InitializeLayerSortingOrders();
    }

    public void ClearPrefabCache()
    {
        windowPrefabCache.Clear();
    }

    public void ClearPool<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        if (windowPool.TryGetValue(windowType, out Queue<UIWindowBase> pool))
        {
            while (pool.Count > 0)
            {
                UIWindowBase window = pool.Dequeue();
                Destroy(window.gameObject);
            }
        }
        windowPool.Remove(windowType);
    }

    public void ClearAllPools()
    {
        foreach (var kvp in windowPool)
        {
            foreach (UIWindowBase window in kvp.Value)
            {
                Destroy(window.gameObject);
            }
        }
        windowPool.Clear();
    }

    public int GetPoolSize<T>() where T : UIWindowBase
    {
        Type windowType = typeof(T);
        return windowPool.TryGetValue(windowType, out Queue<UIWindowBase> pool) ? pool.Count : 0;
    }
}
