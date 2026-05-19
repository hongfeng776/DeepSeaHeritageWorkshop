using UnityEngine;
using System.Collections.Generic;

public class GameMainWindow : UIWindowBase
{
    [Header("Components")]
    [SerializeField] private TopResourceBar topResourceBar;
    [SerializeField] private BottomFunctionBar bottomFunctionBar;
    [SerializeField] private WorkshopArea workshopArea;
    [SerializeField] private WorkshopLevelDisplay workshopLevelDisplay;

    [Header("Panels")]
    [SerializeField] private GameObject[] contentPanels;

    [Header("Test Settings")]
    [SerializeField] private bool enableTestExpGain = true;
    [SerializeField] private long testExpAmount = 50;

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
        InitializeEventListeners();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        RemoveEventListeners();
    }

    private void InitializeComponents()
    {
        if (topResourceBar == null)
        {
            topResourceBar = GetComponentInChildren<TopResourceBar>();
        }
        if (bottomFunctionBar == null)
        {
            bottomFunctionBar = GetComponentInChildren<BottomFunctionBar>();
        }
        if (workshopArea == null)
        {
            workshopArea = GetComponentInChildren<WorkshopArea>();
        }
        if (workshopLevelDisplay == null)
        {
            workshopLevelDisplay = GetComponentInChildren<WorkshopLevelDisplay>();
        }

        if (workshopLevelDisplay == null)
        {
            CreateWorkshopLevelDisplay();
        }

        if (bottomFunctionBar != null)
        {
            bottomFunctionBar.OnButtonClicked += OnFunctionButtonClicked;
            InitializeBottomBarConfigs();
        }

        ShowContentPanel(0);
    }

    private void CreateWorkshopLevelDisplay()
    {
        GameObject levelObj = new GameObject("WorkshopLevelDisplay");
        levelObj.transform.SetParent(transform);
        
        RectTransform rect = levelObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.85f);
        rect.anchorMax = new Vector2(0.35f, 0.98f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image bgImage = levelObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.25f, 0.8f);

        workshopLevelDisplay = levelObj.AddComponent<WorkshopLevelDisplay>();
    }

    private void InitializeBottomBarConfigs()
    {
        if (bottomFunctionBar == null) return;

        var configs = new FunctionButtonConfig[]
        {
            new FunctionButtonConfig { buttonType = FunctionButtonType.Workshop, buttonName = "工坊", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Explore, buttonName = "探索", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Inventory, buttonName = "背包", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Mission, buttonName = "任务", isHighlighted = true },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Shop, buttonName = "商店", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Settings, buttonName = "设置", isHighlighted = false }
        };

        bottomFunctionBar.SetConfigsAndInitialize(configs);
    }

    private void InitializeEventListeners()
    {
        EventManager.Instance.AddListener<ResourceChangedData>(GameEventNames.OnResourceChanged, OnResourceChanged);
    }

    private void RemoveEventListeners()
    {
        EventManager.Instance.RemoveListener<ResourceChangedData>(GameEventNames.OnResourceChanged, OnResourceChanged);
    }

    private void OnFunctionButtonClicked(FunctionButtonType type)
    {
        int panelIndex = (int)type;
        ShowContentPanel(panelIndex);

        switch (type)
        {
            case FunctionButtonType.Workshop:
                workshopArea?.Refresh();
                break;
            case FunctionButtonType.Explore:
                EnterCaveExploration();
                break;
            case FunctionButtonType.Inventory:
                break;
            case FunctionButtonType.Mission:
                bottomFunctionBar?.SetButtonHighlight(FunctionButtonType.Mission, false);
                break;
            case FunctionButtonType.Shop:
                break;
            case FunctionButtonType.Settings:
                UIManager.Instance.OpenWindow<SettingsWindow>();
                break;
        }
    }

    private void EnterCaveExploration()
    {
        Debug.Log("正在进入洞穴探索...");
        SceneLoader.Instance.GoToCaveExploration();
    }

    public void LoadSceneWithTransition(string sceneName, TransitionType transitionType = TransitionType.Fade, float duration = 0.5f)
    {
        SceneLoader.Instance.LoadSceneWithCustomTransition(sceneName, transitionType, duration);
    }

    private void ShowContentPanel(int index)
    {
        if (contentPanels == null || contentPanels.Length == 0) return;

        for (int i = 0; i < contentPanels.Length; i++)
        {
            if (contentPanels[i] != null)
            {
                contentPanels[i].SetActive(i == index);
            }
        }
    }

    protected override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);
        RefreshAll();
        AddTestResources();
        CreateTestExpButton();
    }

    private void CreateTestExpButton()
    {
        if (!enableTestExpGain) return;

        GameObject buttonObj = new GameObject("TestExpButton");
        buttonObj.transform.SetParent(transform);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.85f, 0.05f);
        rect.anchorMax = new Vector2(0.98f, 0.12f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.4f, 0.2f);

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(AddTestExp);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = $"获得经验\n+{testExpAmount}";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 14;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }

    protected override void OnRefresh(object userData = null)
    {
        base.OnRefresh(userData);
        RefreshAll();
    }

    private void RefreshAll()
    {
        topResourceBar?.Refresh();
        workshopArea?.Refresh();
        workshopLevelDisplay?.Refresh();
    }

    public void AddWorkshopExp(long amount)
    {
        WorkshopLevelManager.Instance.AddExp(amount);
    }

    public void AddTestExp()
    {
        if (enableTestExpGain)
        {
            AddWorkshopExp(testExpAmount);
            Debug.Log($"获得测试经验: {testExpAmount}");
        }
    }

    public void SetWorkshopLevel(int level)
    {
        WorkshopLevelManager.Instance.SetLevel(level);
    }

    private void OnResourceChanged(ResourceChangedData data)
    {
    }

    private void AddTestResources()
    {
        ResourceManager.Instance.AddResource(ResourceType.Gold, 1000);
        ResourceManager.Instance.AddResource(ResourceType.Wood, 500);
        ResourceManager.Instance.AddResource(ResourceType.Stone, 300);
        ResourceManager.Instance.AddResource(ResourceType.Iron, 200);
        ResourceManager.Instance.AddResource(ResourceType.Crystal, 100);
        ResourceManager.Instance.AddResource(ResourceType.Energy, 50);
    }

    protected override void OnClose()
    {
        base.OnClose();
    }
}
