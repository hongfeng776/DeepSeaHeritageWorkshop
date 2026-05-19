using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopResourceBar : MonoBehaviour
{
    [Header("Resource Item Prefab")]
    [SerializeField] private ResourceItem resourceItemPrefab;

    [Header("Container")]
    [SerializeField] private Transform resourceContainer;

    [Header("Display Settings")]
    [SerializeField] private ResourceType[] displayResources = new ResourceType[]
    {
        ResourceType.Gold,
        ResourceType.Iron,
        ResourceType.Energy
    };

    private Dictionary<ResourceType, ResourceItem> resourceItems = new Dictionary<ResourceType, ResourceItem>();

    private void Awake()
    {
        InitializeResourceItems();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<Dictionary<ResourceType, long>>(GameEventNames.OnAllResourcesUpdated, OnAllResourcesUpdated);
        EventManager.Instance.AddListener<ResourceChangedData>(GameEventNames.OnResourceChanged, OnResourceChanged);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<Dictionary<ResourceType, long>>(GameEventNames.OnAllResourcesUpdated, OnAllResourcesUpdated);
        EventManager.Instance.RemoveListener<ResourceChangedData>(GameEventNames.OnResourceChanged, OnResourceChanged);
    }

    private void Start()
    {
        Refresh();
    }

    private void InitializeResourceItems()
    {
        if (resourceItemPrefab == null || resourceContainer == null)
        {
            Debug.LogWarning("ResourceItem prefab or container not set!");
            return;
        }

        foreach (Transform child in resourceContainer)
        {
            Destroy(child.gameObject);
        }
        resourceItems.Clear();

        foreach (ResourceType type in displayResources)
        {
            if (!resourceItems.ContainsKey(type))
            {
                ResourceItem item = Instantiate(resourceItemPrefab, resourceContainer);
                item.Initialize(type);
                resourceItems[type] = item;
            }
        }

        Debug.Log($"TopResourceBar initialized with {resourceItems.Count} resources");
    }

    public void Refresh()
    {
        foreach (var type in displayResources)
        {
            long amount = ResourceManager.Instance.GetResource(type);
            if (resourceItems.TryGetValue(type, out ResourceItem item))
            {
                item.UpdateAmount(amount);
            }
        }
    }

    private void OnAllResourcesUpdated(Dictionary<ResourceType, long> resources)
    {
        foreach (var type in displayResources)
        {
            if (resources.TryGetValue(type, out long amount) && resourceItems.TryGetValue(type, out ResourceItem item))
            {
                item.UpdateAmount(amount);
            }
        }
    }

    private void OnResourceChanged(ResourceChangedData data)
    {
        if (resourceItems.TryGetValue(data.Type, out ResourceItem item))
        {
            item.UpdateAmount(data.NewAmount, data.Delta);
        }
    }
}

public class ResourceItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text amountText;
    [SerializeField] private Text deltaText;
    [SerializeField] private Animator animator;

    private ResourceType resourceType;
    private long currentAmount;

    private const float DeltaDisplayDuration = 2f;
    private float deltaTimer;

    private static readonly Dictionary<ResourceType, string> ResourceNames = new Dictionary<ResourceType, string>
    {
        { ResourceType.Gold, "金币" },
        { ResourceType.Wood, "木材" },
        { ResourceType.Stone, "石材" },
        { ResourceType.Iron, "矿石" },
        { ResourceType.Crystal, "水晶" },
        { ResourceType.Energy, "遗迹能量" }
    };

    public void Initialize(ResourceType type)
    {
        resourceType = type;
        gameObject.name = $"ResourceItem_{type}";
        
        if (nameText != null && ResourceNames.TryGetValue(type, out string resourceName))
        {
            nameText.text = resourceName;
        }
        
        LoadIcon(type);
        deltaText?.gameObject.SetActive(false);
    }

    private void LoadIcon(ResourceType type)
    {
        if (iconImage == null) return;

        string iconName = type.ToString().ToLower();
    }

    public void UpdateAmount(long newAmount, long delta = 0)
    {
        currentAmount = newAmount;
        if (amountText != null)
        {
            amountText.text = FormatNumber(currentAmount);
        }

        if (delta != 0 && deltaText != null)
        {
            ShowDelta(delta);
        }
    }

    private void ShowDelta(long delta)
    {
        deltaText.gameObject.SetActive(true);
        deltaText.text = delta > 0 ? $"+{FormatNumber(delta)}" : $"{FormatNumber(delta)}";
        deltaText.color = delta > 0 ? Color.green : Color.red;

        deltaTimer = DeltaDisplayDuration;
        animator?.SetTrigger("ShowDelta");
    }

    private void Update()
    {
        if (deltaTimer > 0)
        {
            deltaTimer -= Time.deltaTime;
            if (deltaTimer <= 0)
            {
                deltaText?.gameObject.SetActive(false);
            }
        }
    }

    private string FormatNumber(long number)
    {
        if (number >= 100000000)
        {
            return (number / 100000000f).ToString("0.#") + "亿";
        }
        if (number >= 10000)
        {
            return (number / 10000f).ToString("0.#") + "万";
        }
        return number.ToString();
    }
}
