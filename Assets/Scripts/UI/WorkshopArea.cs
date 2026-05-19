using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WorkshopItemType
{
    RepairStation,
    ResearchLab,
    Storage,
    CraftingTable,
    ExhibitionHall
}

[Serializable]
public class WorkshopItemData
{
    public WorkshopItemType itemType;
    public string itemName;
    public string description;
    public int level;
    public int maxLevel;
    public bool isUnlocked;
    public Dictionary<ResourceType, long> upgradeCosts;
    public Sprite icon;
}

public class WorkshopArea : MonoBehaviour
{
    [Header("Workshop Item Prefab")]
    [SerializeField] private WorkshopItem workshopItemPrefab;

    [Header("Container")]
    [SerializeField] private Transform itemContainer;

    [Header("Detail Panel")]
    [SerializeField] private WorkshopDetailPanel detailPanel;

    private List<WorkshopItemData> workshopItemsData;
    private Dictionary<WorkshopItemType, WorkshopItem> workshopItems = new Dictionary<WorkshopItemType, WorkshopItem>();

    private void Awake()
    {
        InitializeWorkshopData();
        InitializeWorkshopItems();
    }

    private void Start()
    {
        Refresh();
    }

    private void InitializeWorkshopData()
    {
        workshopItemsData = new List<WorkshopItemData>
        {
            CreateWorkshopItemData(WorkshopItemType.RepairStation, "修复台", "修复深海文物的设施", 1, 5, true),
            CreateWorkshopItemData(WorkshopItemType.ResearchLab, "研究室", "研究文物历史的实验室", 1, 5, true),
            CreateWorkshopItemData(WorkshopItemType.Storage, "储藏室", "存放文物和材料的空间", 1, 5, true),
            CreateWorkshopItemData(WorkshopItemType.CraftingTable, "制作台", "制作工具和材料的工作台", 0, 5, false),
            CreateWorkshopItemData(WorkshopItemType.ExhibitionHall, "展览厅", "展示修复完成的文物", 0, 5, false)
        };
    }

    private WorkshopItemData CreateWorkshopItemData(WorkshopItemType type, string name, string desc, int level, int maxLevel, bool unlocked)
    {
        return new WorkshopItemData
        {
            itemType = type,
            itemName = name,
            description = desc,
            level = level,
            maxLevel = maxLevel,
            isUnlocked = unlocked,
            upgradeCosts = new Dictionary<ResourceType, long>
            {
                { ResourceType.Gold, 100 * (level + 1) },
                { ResourceType.Wood, 50 * (level + 1) },
                { ResourceType.Stone, 30 * (level + 1) }
            }
        };
    }

    private void InitializeWorkshopItems()
    {
        if (workshopItemPrefab == null || itemContainer == null)
        {
            Debug.LogWarning("WorkshopItem prefab or container not set!");
            return;
        }

        foreach (var itemData in workshopItemsData)
        {
            if (!workshopItems.ContainsKey(itemData.itemType))
            {
                WorkshopItem item = Instantiate(workshopItemPrefab, itemContainer);
                item.Initialize(itemData, OnWorkshopItemClicked);
                workshopItems[itemData.itemType] = item;
            }
        }
    }

    private void OnWorkshopItemClicked(WorkshopItemData data)
    {
        if (detailPanel != null)
        {
            detailPanel.Show(data, OnUpgradeClicked, OnCloseDetail);
        }
    }

    private void OnUpgradeClicked(WorkshopItemData data)
    {
        if (CanAffordUpgrade(data))
        {
            SpendUpgradeResources(data);
            data.level++;
            data.upgradeCosts = new Dictionary<ResourceType, long>
            {
                { ResourceType.Gold, 100 * (data.level + 1) },
                { ResourceType.Wood, 50 * (data.level + 1) },
                { ResourceType.Stone, 30 * (data.level + 1) }
            };

            if (workshopItems.TryGetValue(data.itemType, out WorkshopItem item))
            {
                item.Refresh(data);
            }

            detailPanel?.Refresh(data);
        }
        else
        {
            Debug.LogWarning("Not enough resources to upgrade!");
        }
    }

    private bool CanAffordUpgrade(WorkshopItemData data)
    {
        return ResourceManager.Instance.CanAfford(data.upgradeCosts);
    }

    private void SpendUpgradeResources(WorkshopItemData data)
    {
        ResourceManager.Instance.SpendResources(data.upgradeCosts);
    }

    private void OnCloseDetail()
    {
        detailPanel?.Hide();
    }

    public void Refresh()
    {
        foreach (var itemData in workshopItemsData)
        {
            if (workshopItems.TryGetValue(itemData.itemType, out WorkshopItem item))
            {
                item.Refresh(itemData);
            }
        }
    }

    public void UnlockItem(WorkshopItemType type)
    {
        var itemData = workshopItemsData.Find(x => x.itemType == type);
        if (itemData != null && !itemData.isUnlocked)
        {
            itemData.isUnlocked = true;
            itemData.level = 1;
            if (workshopItems.TryGetValue(type, out WorkshopItem item))
            {
                item.Refresh(itemData);
            }
        }
    }
}

public class WorkshopItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button button;
    [SerializeField] private Animator animator;

    public WorkshopItemData Data { get; private set; }
    private Action<WorkshopItemData> clickCallback;

    public void Initialize(WorkshopItemData data, Action<WorkshopItemData> callback)
    {
        Data = data;
        clickCallback = callback;

        Refresh(data);
        button?.onClick.AddListener(OnClick);
    }

    public void Refresh(WorkshopItemData data)
    {
        Data = data;

        if (nameText != null)
        {
            nameText.text = data.itemName;
        }

        if (levelText != null)
        {
            levelText.text = data.isUnlocked ? $"Lv.{data.level}/{data.maxLevel}" : "未解锁";
        }

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!data.isUnlocked);
        }

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }
    }

    private void OnClick()
    {
        clickCallback?.Invoke(Data);
        animator?.SetTrigger("Click");
    }

    private void OnDestroy()
    {
        button?.onClick.RemoveAllListeners();
    }
}

public class WorkshopDetailPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Transform costContainer;
    [SerializeField] private ResourceCostItem costItemPrefab;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Animator animator;

    private WorkshopItemData currentData;
    private Action<WorkshopItemData> upgradeCallback;
    private Action closeCallback;
    private List<ResourceCostItem> costItems = new List<ResourceCostItem>();

    private void Awake()
    {
        upgradeButton?.onClick.AddListener(OnUpgradeClicked);
        closeButton?.onClick.AddListener(OnCloseClicked);
    }

    public void Show(WorkshopItemData data, Action<WorkshopItemData> onUpgrade, Action onClose)
    {
        currentData = data;
        upgradeCallback = onUpgrade;
        closeCallback = onClose;

        Refresh(data);
        gameObject.SetActive(true);
        animator?.SetTrigger("Show");
    }

    public void Refresh(WorkshopItemData data)
    {
        currentData = data;

        if (nameText != null)
        {
            nameText.text = data.itemName;
        }

        if (levelText != null)
        {
            levelText.text = data.isUnlocked ? $"Lv.{data.level}/{data.maxLevel}" : "未解锁";
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }

        UpdateCosts(data);
        UpdateUpgradeButton(data);
    }

    private void UpdateCosts(WorkshopItemData data)
    {
        if (costContainer == null || costItemPrefab == null) return;

        foreach (var item in costItems)
        {
            Destroy(item.gameObject);
        }
        costItems.Clear();

        foreach (var cost in data.upgradeCosts)
        {
            ResourceCostItem item = Instantiate(costItemPrefab, costContainer);
            item.Initialize(cost.Key, cost.Value);
            costItems.Add(item);
        }
    }

    private void UpdateUpgradeButton(WorkshopItemData data)
    {
        if (upgradeButton == null) return;

        bool canUpgrade = data.isUnlocked && data.level < data.maxLevel && ResourceManager.Instance.CanAfford(data.upgradeCosts);
        upgradeButton.interactable = canUpgrade;

        var buttonText = upgradeButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            if (!data.isUnlocked)
            {
                buttonText.text = "未解锁";
            }
            else if (data.level >= data.maxLevel)
            {
                buttonText.text = "已满级";
            }
            else
            {
                buttonText.text = "升级";
            }
        }
    }

    private void OnUpgradeClicked()
    {
        upgradeCallback?.Invoke(currentData);
        animator?.SetTrigger("Upgrade");
    }

    private void OnCloseClicked()
    {
        closeCallback?.Invoke();
    }

    public void Hide()
    {
        animator?.SetTrigger("Hide");
        Invoke(nameof(DeactivatePanel), 0.3f);
    }

    private void DeactivatePanel()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        upgradeButton?.onClick.RemoveAllListeners();
        closeButton?.onClick.RemoveAllListeners();
    }
}

public class ResourceCostItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;

    public void Initialize(ResourceType type, long amount)
    {
        if (amountText != null)
        {
            long current = ResourceManager.Instance.GetResource(type);
            amountText.text = $"{current}/{amount}";
            amountText.color = current >= amount ? Color.white : Color.red;
        }
    }
}
