using UnityEngine;

public class CollectibleResource : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ResourceType resourceType = ResourceType.Stone;
    [SerializeField] private int amount = 10;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatFrequency = 1f;
    [SerializeField] private float bobbleIntensity = 0.1f;
    [SerializeField] private float collectAnimationTime = 0.3f;

    [Header("Visual")]
    [SerializeField] private bool autoSetColorByType = true;
    [SerializeField] private Color resourceColor = Color.gray;
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;
    private Vector3 startScale;
    private float floatTime;
    private bool isCollecting;
    private float collectTimer;

    public ResourceType ResourceType => resourceType;
    public int Amount => amount;

    private void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
        
        if (autoSetColorByType)
        {
            SetColorByResourceType();
        }
        
        CreateVisual();
        CreateCollectEffects();
    }

    private void SetColorByResourceType()
    {
        resourceColor = GetResourceColor(resourceType);
    }

    public static Color GetResourceColor(ResourceType type)
    {
        return type switch
        {
            ResourceType.Gold => Color.yellow,
            ResourceType.Wood => new Color(0.6f, 0.4f, 0.2f),
            ResourceType.Stone => Color.gray,
            ResourceType.Iron => new Color(0.7f, 0.5f, 0.3f),
            ResourceType.Crystal => Color.magenta,
            ResourceType.Energy => Color.cyan,
            ResourceType.IronOre => new Color(0.6f, 0.4f, 0.2f),
            ResourceType.CopperOre => new Color(0.8f, 0.5f, 0.2f),
            ResourceType.SilverOre => new Color(0.9f, 0.9f, 0.95f),
            ResourceType.GoldOre => new Color(1f, 0.85f, 0f),
            ResourceType.CrystalOre => new Color(0.8f, 0.3f, 1f),
            ResourceType.RelicFragment => new Color(0.3f, 0.8f, 0.8f),
            ResourceType.AncientRelic => new Color(0.2f, 0.6f, 1f),
            ResourceType.RareRelic => new Color(1f, 0.3f, 0.8f),
            _ => Color.white
        };
    }

    private void Update()
    {
        if (isCollecting)
        {
            UpdateCollectAnimation();
            return;
        }

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        
        floatTime += Time.deltaTime * floatFrequency;
        float floatOffset = Mathf.Sin(floatTime) * floatAmplitude;
        float bobbleOffset = Mathf.Sin(floatTime * 2f) * bobbleIntensity;
        transform.position = startPosition + new Vector3(bobbleOffset, floatOffset, 0);
    }

    private void CreateVisual()
    {
        if (GetComponent<Renderer>() == null)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.material.color = resourceColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", resourceColor * 0.3f);
            
            Destroy(visual.GetComponent<Collider>());
        }

        if (GetComponent<Collider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.8f;
        }

        Light glowLight = GetComponent<Light>();
        if (glowLight == null)
        {
            glowLight = gameObject.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 1.5f;
            glowLight.intensity = 0.8f;
            glowLight.color = resourceColor;
        }
    }

    private void CreateCollectEffects()
    {
        if (collectParticles == null)
        {
            GameObject particleObj = new GameObject("CollectParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            collectParticles = particleObj.AddComponent<ParticleSystem>();
            var main = collectParticles.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = 0.2f;
            main.startColor = resourceColor;
            main.maxParticles = 20;
            main.playOnAwake = false;

            var emission = collectParticles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 15)
            });

            var shape = collectParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollecting)
        {
            StartCollect();
        }
    }

    private void StartCollect()
    {
        isCollecting = true;
        collectTimer = collectAnimationTime;
        
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }
        else if (collectParticles != null)
        {
            collectParticles.Play();
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        EventManager.Instance?.TriggerEvent(GameEventNames.OnResourceCollected, new ResourceCollectedData(resourceType, amount, transform.position));
        Debug.Log($"开始收集: {resourceType} x{amount}");
    }

    private void UpdateCollectAnimation()
    {
        collectTimer -= Time.deltaTime;
        
        float progress = 1f - (collectTimer / collectAnimationTime);
        float scale = Mathf.Lerp(1f, 0f, progress);
        transform.localScale = startScale * scale;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Vector3 targetPos = playerObj.transform.position + Vector3.up * 1.5f;
            transform.position = Vector3.Lerp(startPosition, targetPos, progress * 2f);
        }

        if (collectTimer <= 0f)
        {
            CompleteCollect();
        }
    }

    private void CompleteCollect()
    {
        CaveExplorationManager.Instance?.CollectResource(resourceType, amount);
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(resourceType, amount);
        }
        else
        {
            Debug.LogWarning("InventoryManager not found, creating one...");
            GameObject inventoryObj = new GameObject("InventoryManager");
            inventoryObj.AddComponent<InventoryManager>();
            InventoryManager.Instance?.AddItem(resourceType, amount);
        }

        Debug.Log($"✅ 收集完成: {resourceType} x{amount}");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = resourceColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

public class ResourceCollectedData
{
    public ResourceType ResourceType { get; }
    public int Amount { get; }
    public Vector3 Position { get; }

    public ResourceCollectedData(ResourceType type, int amount, Vector3 position)
    {
        ResourceType = type;
        Amount = amount;
        Position = position;
    }
}
