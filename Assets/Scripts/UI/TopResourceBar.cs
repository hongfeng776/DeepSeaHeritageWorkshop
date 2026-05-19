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

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (!resourceItems.ContainsKey(type))
            {
                ResourceItem item = Instantiate(resourceItemPrefab, resourceContainer);
                item.Initialize(type);
                resourceItems[type] = item;
            }
        }
    }

    public void Refresh()
    {
        var allResources = ResourceManager.Instance.GetAllResources();
        foreach (var resource in allResources)
        {
            if (resourceItems.TryGetValue(resource.Key, out ResourceItem item))
            {
                item.UpdateAmount(resource.Value);
            }
        }
    }

    private void OnAllResourcesUpdated(Dictionary<ResourceType, long> resources)
    {
        foreach (var resource in resources)
        {
            if (resourceItems.TryGetValue(resource.Key, out ResourceItem item))
            {
                item.UpdateAmount(resource.Value);
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
    [SerializeField] private Text amountText;
    [SerializeField] private Text deltaText;
    [SerializeField] private Animator animator;

    private ResourceType resourceType;
    private long currentAmount;

    private const float DeltaDisplayDuration = 2f;
    private float deltaTimer;

    public void Initialize(ResourceType type)
    {
        resourceType = type;
        gameObject.name = $"ResourceItem_{type}";
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
