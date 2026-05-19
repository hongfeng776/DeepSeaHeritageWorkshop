using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoSingleton<ResourceManager>
{
    private Dictionary<ResourceType, long> resources = new Dictionary<ResourceType, long>();

    protected override void Awake()
    {
        base.Awake();
        InitializeResources();
    }

    private void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
    }

    public long GetResource(ResourceType type)
    {
        return resources.TryGetValue(type, out long amount) ? amount : 0;
    }

    public Dictionary<ResourceType, long> GetAllResources()
    {
        return new Dictionary<ResourceType, long>(resources);
    }

    public void AddResource(ResourceType type, long amount)
    {
        if (amount <= 0) return;

        long oldAmount = resources[type];
        resources[type] += amount;
        
        NotifyResourceChanged(type, oldAmount, resources[type]);
    }

    public bool SpendResource(ResourceType type, long amount)
    {
        if (amount <= 0) return false;
        if (resources[type] < amount) return false;

        long oldAmount = resources[type];
        resources[type] -= amount;
        
        NotifyResourceChanged(type, oldAmount, resources[type]);
        return true;
    }

    public bool CanAfford(Dictionary<ResourceType, long> costs)
    {
        foreach (var cost in costs)
        {
            if (resources[cost.Key] < cost.Value)
            {
                return false;
            }
        }
        return true;
    }

    public bool SpendResources(Dictionary<ResourceType, long> costs)
    {
        if (!CanAfford(costs)) return false;

        foreach (var cost in costs)
        {
            long oldAmount = resources[cost.Key];
            resources[cost.Key] -= cost.Value;
            NotifyResourceChanged(cost.Key, oldAmount, resources[cost.Key]);
        }
        return true;
    }

    private void NotifyResourceChanged(ResourceType type, long oldAmount, long newAmount)
    {
        var data = new ResourceChangedData(type, oldAmount, newAmount);
        EventManager.Instance.TriggerEvent(GameEventNames.OnResourceChanged, data);
        EventManager.Instance.TriggerEvent(GameEventNames.OnAllResourcesUpdated, GetAllResources());
    }
}
