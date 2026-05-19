using UnityEngine;

public class CollectibleResource : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ResourceType resourceType = ResourceType.Stone;
    [SerializeField] private int amount = 10;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatFrequency = 1f;

    [Header("Visual")]
    [SerializeField] private bool autoSetColorByType = true;
    [SerializeField] private Color resourceColor = Color.gray;

    private Vector3 startPosition;
    private float floatTime;

    private void Start()
    {
        startPosition = transform.position;
        
        if (autoSetColorByType)
        {
            SetColorByResourceType();
        }
        
        CreateVisual();
    }

    private void SetColorByResourceType()
    {
        switch (resourceType)
        {
            case ResourceType.Gold:
                resourceColor = Color.yellow;
                break;
            case ResourceType.Wood:
                resourceColor = new Color(0.6f, 0.4f, 0.2f);
                break;
            case ResourceType.Stone:
                resourceColor = Color.gray;
                break;
            case ResourceType.Iron:
                resourceColor = new Color(0.7f, 0.5f, 0.3f);
                break;
            case ResourceType.Crystal:
                resourceColor = Color.magenta;
                break;
            case ResourceType.Energy:
                resourceColor = Color.cyan;
                break;
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        
        floatTime += Time.deltaTime * floatFrequency;
        float floatOffset = Mathf.Sin(floatTime) * floatAmplitude;
        transform.position = startPosition + new Vector3(0, floatOffset, 0);
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
            
            Destroy(visual.GetComponent<Collider>());
        }

        if (GetComponent<Collider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.8f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        CaveExplorationManager.Instance?.CollectResource(resourceType, amount);
        Debug.Log($"收集了 {amount} {resourceType}");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = resourceColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
