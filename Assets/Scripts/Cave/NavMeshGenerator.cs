using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CaveGenerator))]
public class NavMeshGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float bakeDelay = 0.5f;
    [SerializeField] private LayerMask walkableLayer = -1;
    [SerializeField] private float agentRadius = 0.5f;
    [SerializeField] private float agentHeight = 2f;

    private CaveGenerator caveGenerator;
    private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        caveGenerator = GetComponent<CaveGenerator>();
    }

    private void Start()
    {
        if (navMeshSurface == null)
        {
            CreateNavMeshSurface();
        }
    }

    private void CreateNavMeshSurface()
    {
        GameObject navObj = new GameObject("NavMeshSurface");
        navObj.transform.SetParent(transform);
        navMeshSurface = navObj.AddComponent<NavMeshSurface>();

        navMeshSurface.agentTypeID = 0;
        navMeshSurface.collectObjects = CollectObjects.All;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        navMeshSurface.layerMask = walkableLayer;

        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
        settings.agentRadius = agentRadius;
        settings.agentHeight = agentHeight;
        navMeshSurface.overrideVoxelSize = true;
        navMeshSurface.voxelSize = 0.1f;
    }

    public void BakeNavMesh()
    {
        if (navMeshSurface == null)
        {
            CreateNavMeshSurface();
        }

        Invoke(nameof(PerformBake), bakeDelay);
    }

    private void PerformBake()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh 烘焙完成！");
        }
    }

    public void BakeNavMeshImmediate()
    {
        if (navMeshSurface == null)
        {
            CreateNavMeshSurface();
        }

        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh 立即烘焙完成！");
    }
}
