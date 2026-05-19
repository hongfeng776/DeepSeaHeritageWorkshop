using UnityEngine;

public class CaveExplorationManager : MonoSingleton<CaveExplorationManager>
{
    [Header("Settings")]
    [SerializeField] private Vector3 playerStartPosition = new Vector3(0, 1, 0);
    [SerializeField] private string caveName = "神秘洞穴";
    [SerializeField] private int baseOxygen = 100;
    [SerializeField] private float oxygenConsumptionRate = 1f;
    [SerializeField] private bool useProceduralGeneration = true;
    [SerializeField] private bool spawnEnemies = true;
    [SerializeField] private float enemySpawnDelay = 1f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private CaveGenerator caveGenerator;
    [SerializeField] private EnemySpawnManager enemySpawnManager;

    private int currentOxygen;
    private float explorationTime;
    private bool isExploring;

    public int CurrentOxygen => currentOxygen;
    public float ExplorationTime => explorationTime;
    public string CaveName => caveName;
    public bool IsExploring => isExploring;
    public CaveGenerator CaveGenerator => caveGenerator;
    public EnemySpawnManager EnemySpawnManager => enemySpawnManager;

    protected override void Awake()
    {
        base.Awake();
        currentOxygen = baseOxygen;

        if (caveGenerator == null)
        {
            caveGenerator = FindObjectOfType<CaveGenerator>();
            if (caveGenerator == null && useProceduralGeneration)
            {
                GameObject generatorObj = new GameObject("CaveGenerator");
                generatorObj.transform.SetParent(transform);
                caveGenerator = generatorObj.AddComponent<CaveGenerator>();
            }
        }

        if (enemySpawnManager == null && spawnEnemies)
        {
            enemySpawnManager = FindObjectOfType<EnemySpawnManager>();
            if (enemySpawnManager == null)
            {
                GameObject spawnObj = new GameObject("EnemySpawnManager");
                spawnObj.transform.SetParent(transform);
                enemySpawnManager = spawnObj.AddComponent<EnemySpawnManager>();
            }
        }
    }

    private void Start()
    {
        InitializeExploration();
    }

    private void Update()
    {
        if (isExploring)
        {
            explorationTime += Time.deltaTime;
            UpdateOxygen();
        }
    }

    private void InitializeExploration()
    {
        Debug.Log($"初始化洞穴探索: {caveName}");
        isExploring = true;
        explorationTime = 0f;
        currentOxygen = baseOxygen;

        if (useProceduralGeneration && caveGenerator != null)
        {
            caveGenerator.GenerateCave();
            playerStartPosition = caveGenerator.StartPosition + Vector3.up * 1f;
            Debug.Log($"洞穴生成完成，起始位置: {playerStartPosition}");
        }

        if (playerController == null)
        {
            FindOrCreatePlayer();
        }

        if (playerStartPoint != null)
        {
            playerStartPosition = playerStartPoint.position;
        }

        playerController?.SetPosition(playerStartPosition);

        if (spawnEnemies && enemySpawnManager != null)
        {
            Invoke(nameof(StartEnemySpawning), enemySpawnDelay);
        }

        EventManager.Instance.TriggerEvent(GameEventNames.OnCaveExplorationStarted, this);
    }

    private void StartEnemySpawning()
    {
        if (enemySpawnManager != null)
        {
            enemySpawnManager.StartSpawningEnemies();
        }
    }

    private void FindOrCreatePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj == null)
        {
            playerObj = CreatePlayer();
        }

        playerController = playerObj.GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            playerController = playerObj.AddComponent<PlayerController>();
        }

        CharacterController controller = playerObj.GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = playerObj.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0, 1f, 0);
        }

        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerObj.AddComponent<PlayerHealth>();
        }

        CapsuleCollider collider = playerObj.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = playerObj.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
            collider.isTrigger = false;
        }

        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = playerObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private GameObject CreatePlayer()
    {
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.transform.position = playerStartPosition;

        GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visualObj.transform.SetParent(playerObj.transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = new Vector3(1, 1, 1);
        Destroy(visualObj.GetComponent<Collider>());

        Renderer renderer = visualObj.GetComponent<Renderer>();
        renderer.material.color = new Color(0.2f, 0.6f, 0.9f);

        return playerObj;
    }

    private void UpdateOxygen()
    {
        float consumption = oxygenConsumptionRate * Time.deltaTime;
        currentOxygen = Mathf.Max(0, currentOxygen - Mathf.RoundToInt(consumption));

        if (currentOxygen <= 0)
        {
            OnOxygenDepleted();
        }
    }

    private void OnOxygenDepleted()
    {
        Debug.LogWarning("氧气耗尽！强制返回");
        ExitExploration();
    }

    public void AddOxygen(int amount)
    {
        currentOxygen = Mathf.Min(baseOxygen, currentOxygen + amount);
        EventManager.Instance.TriggerEvent(GameEventNames.OnCaveOxygenChanged, currentOxygen);
    }

    public void ExitExploration()
    {
        if (!isExploring) return;

        isExploring = false;
        Debug.Log($"探索结束，用时: {explorationTime:F1}秒");

        EventManager.Instance.TriggerEvent(GameEventNames.OnCaveExplorationEnded, this);
        SceneLoader.Instance.ReturnToMainScene();
    }

    public void CollectResource(ResourceType type, int amount)
    {
        ResourceManager.Instance.AddResource(type, amount);
        EventManager.Instance.TriggerEvent(GameEventNames.OnCaveResourceCollected, new CaveResourceData(type, amount));
    }
}

public class CaveResourceData
{
    public ResourceType ResourceType { get; }
    public int Amount { get; }

    public CaveResourceData(ResourceType type, int amount)
    {
        ResourceType = type;
        Amount = amount;
    }
}
