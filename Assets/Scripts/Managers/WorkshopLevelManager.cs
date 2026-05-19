using UnityEngine;

public class WorkshopLevelManager : MonoSingleton<WorkshopLevelManager>
{
    private WorkshopLevelData levelData = new WorkshopLevelData();

    public int CurrentLevel => levelData.Level;
    public long CurrentExp => levelData.CurrentExp;
    public long ExpToNextLevel => levelData.ExpToNextLevel;
    public float ExpPercentage => levelData.GetExpPercentage();

    protected override void Awake()
    {
        base.Awake();
        InitializeLevelData();
    }

    private void InitializeLevelData()
    {
        levelData = new WorkshopLevelData();
    }

    public void AddExp(long amount)
    {
        if (amount <= 0) return;

        levelData.AddExp(amount);
        EventManager.Instance.TriggerEvent(GameEventNames.OnWorkshopExpChanged, levelData);

        while (levelData.CanLevelUp())
        {
            levelData.LevelUp();
            EventManager.Instance.TriggerEvent(GameEventNames.OnWorkshopLevelUp, levelData.Level);
        }

        EventManager.Instance.TriggerEvent(GameEventNames.OnWorkshopLevelUpdated, levelData);
    }

    public void SetLevel(int targetLevel)
    {
        if (targetLevel <= 0) return;

        levelData = new WorkshopLevelData();
        levelData.Level = targetLevel;
        levelData.ExpToNextLevel = CalculateExpToNextLevel(targetLevel);

        EventManager.Instance.TriggerEvent(GameEventNames.OnWorkshopLevelUpdated, levelData);
    }

    private long CalculateExpToNextLevel(int currentLevel)
    {
        return (long)(100 * Mathf.Pow(1.5f, currentLevel - 1));
    }

    public WorkshopLevelData GetLevelData()
    {
        return levelData;
    }
}
