using System;

[Serializable]
public class WorkshopLevelData
{
    public int Level { get; set; }
    public long CurrentExp { get; set; }
    public long ExpToNextLevel { get; set; }
    public long TotalExp { get; set; }

    public WorkshopLevelData()
    {
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = 100;
        TotalExp = 0;
    }

    public float GetExpPercentage()
    {
        if (ExpToNextLevel <= 0) return 1f;
        return (float)CurrentExp / ExpToNextLevel;
    }

    public bool CanLevelUp()
    {
        return CurrentExp >= ExpToNextLevel;
    }

    public void LevelUp()
    {
        Level++;
        CurrentExp -= ExpToNextLevel;
        ExpToNextLevel = CalculateExpToNextLevel(Level);
    }

    private long CalculateExpToNextLevel(int currentLevel)
    {
        return (long)(100 * Math.Pow(1.5f, currentLevel - 1));
    }

    public void AddExp(long amount)
    {
        CurrentExp += amount;
        TotalExp += amount;
    }
}
