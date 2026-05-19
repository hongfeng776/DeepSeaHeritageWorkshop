using System;
using System.Collections.Generic;

[Serializable]
public class ResourceData
{
    public ResourceType Type;
    public long Amount;

    public ResourceData(ResourceType type, long amount)
    {
        Type = type;
        Amount = amount;
    }
}

[Serializable]
public class ResourceChangedData
{
    public ResourceType Type;
    public long OldAmount;
    public long NewAmount;
    public long Delta;

    public ResourceChangedData(ResourceType type, long oldAmount, long newAmount)
    {
        Type = type;
        OldAmount = oldAmount;
        NewAmount = newAmount;
        Delta = newAmount - oldAmount;
    }
}
