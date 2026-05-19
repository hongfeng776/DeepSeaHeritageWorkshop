using UnityEngine;
using System.Collections.Generic;

public class GameMainWindow : UIWindowBase
{
    [Header("Components")]
    [SerializeField] private TopResourceBar topResourceBar;
    [SerializeField] private BottomFunctionBar bottomFunctionBar;
    [SerializeField] private WorkshopArea workshopArea;

    [Header("Panels")]
    [SerializeField] private GameObject[] contentPanels;

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

        if (bottomFunctionBar != null)
        {
            bottomFunctionBar.OnButtonClicked += OnFunctionButtonClicked;
        }

        InitializeBottomBarConfigs();
        ShowContentPanel(0);
    }

    private void InitializeBottomBarConfigs()
    {
        if (bottomFunctionBar == null) return;

        var configs = new List<FunctionButtonConfig>
        {
            new FunctionButtonConfig { buttonType = FunctionButtonType.Workshop, buttonName = "工坊", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Explore, buttonName = "探索", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Inventory, buttonName = "背包", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Mission, buttonName = "任务", isHighlighted = true },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Shop, buttonName = "商店", isHighlighted = false },
            new FunctionButtonConfig { buttonType = FunctionButtonType.Settings, buttonName = "设置", isHighlighted = false }
        };

        var configField = bottomFunctionBar.GetType().GetField("buttonConfigs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configField != null)
        {
            configField.SetValue(bottomFunctionBar, configs.ToArray());
        }
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
