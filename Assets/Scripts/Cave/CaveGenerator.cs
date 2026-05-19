using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum TileType
{
    Empty,
    Floor,
    Wall,
    Obstacle,
    Collectible,
    OreNode,
    StartPoint,
    ExitPoint
}

[System.Serializable]
public class Room
{
    public int x;
    public int y;
    public int width;
    public int height;
    public bool isConnected;
    public List<Room> connectedRooms;

    public Room(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        isConnected = false;
        connectedRooms = new List<Room>();
    }

    public Vector2 Center => new Vector2(x + width / 2f, y + height / 2f);

    public bool Intersects(Room other)
    {
        return x < other.x + other.width &&
               x + width > other.x &&
               y < other.y + other.height &&
               y + height > other.y;
    }
}

[System.Serializable]
public class Corridor
{
    public List<Vector2Int> tiles;

    public Corridor()
    {
        tiles = new List<Vector2Int>();
    }
}

public class CaveGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 50;
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private int seed = 0;
    [SerializeField] private bool useRandomSeed = true;

    [Header("Room Settings")]
    [SerializeField] private int roomCount = 8;
    [SerializeField] private int minRoomSize = 5;
    [SerializeField] private int maxRoomSize = 12;
    [SerializeField] private int roomPadding = 2;

    [Header("Tile Settings")]
    [SerializeField] private float tileSize = 3f;
    [SerializeField] private float wallHeight = 4f;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject[] collectiblePrefabs;
    [SerializeField] private GameObject exitPrefab;

    [Header("Generation Settings")]
    [SerializeField] [Range(0, 100)] private int obstacleDensity = 15;
    [SerializeField] [Range(0, 100)] private int collectibleDensity = 8;
    [SerializeField] [Range(0, 100)] private int oreNodeDensity = 10;
    [SerializeField] private int corridorWidth = 2;
    [SerializeField] private bool bakeNavMesh = true;
    [SerializeField] private float navMeshBakeDelay = 0.3f;

    private TileType[,] mapTiles;
    private List<Room> rooms;
    private List<Corridor> corridors;
    private Transform mapParent;
    private System.Random random;
    private NavMeshGenerator navMeshGenerator;

    public List<Room> Rooms => rooms;
    public List<Corridor> Corridors => corridors;
    public TileType[,] MapTiles => mapTiles;
    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public float TileSize => tileSize;
    public Vector3 StartPosition { get; private set; }
    public Vector3 ExitPosition { get; private set; }

    private void Awake()
    {
        InitializeGenerator();
    }

    private void Start()
    {
        if (bakeNavMesh)
        {
            navMeshGenerator = GetComponent<NavMeshGenerator>();
            if (navMeshGenerator == null)
            {
                navMeshGenerator = gameObject.AddComponent<NavMeshGenerator>();
            }
        }
    }

    public void InitializeGenerator()
    {
        if (useRandomSeed)
        {
            seed = System.DateTime.Now.GetHashCode();
        }
        random = new System.Random(seed);
        rooms = new List<Room>();
        corridors = new List<Corridor>();
        mapTiles = new TileType[mapWidth, mapHeight];
    }

    public void GenerateCave()
    {
        Debug.Log($"开始生成洞穴，Seed: {seed}");

        ClearMap();
        InitializeMap();
        GenerateRooms();
        ConnectRooms();
        GenerateCorridors();
        GenerateWalls();
        GenerateObstacles();
        SetStartAndExitPoints();
        GenerateCollectibles();
        GenerateOreNodes();
        BuildMap();

        Debug.Log($"洞穴生成完成！房间数: {rooms.Count}, 走廊数: {corridors.Count}");
    }

    private void ClearMap()
    {
        if (mapParent != null)
        {
            Destroy(mapParent.gameObject);
        }

        GameObject mapObject = new GameObject("CaveMap");
        mapParent = mapObject.transform;
        mapParent.SetParent(transform);
    }

    private void InitializeMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapTiles[x, y] = TileType.Wall;
            }
        }
    }

    private void GenerateRooms()
    {
        int attempts = 0;
        int maxAttempts = roomCount * 50;

        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            int roomWidth = random.Next(minRoomSize, maxRoomSize + 1);
            int roomHeight = random.Next(minRoomSize, maxRoomSize + 1);
            int roomX = random.Next(roomPadding, mapWidth - roomWidth - roomPadding);
            int roomY = random.Next(roomPadding, mapHeight - roomHeight - roomPadding);

            Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight);

            bool overlaps = false;
            foreach (Room existingRoom in rooms)
            {
                Room paddedRoom = new Room(
                    existingRoom.x - roomPadding,
                    existingRoom.y - roomPadding,
                    existingRoom.width + roomPadding * 2,
                    existingRoom.height + roomPadding * 2
                );

                if (newRoom.Intersects(paddedRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }

        Debug.Log($"生成了 {rooms.Count} 个房间，尝试了 {attempts} 次");
    }

    private void CarveRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                {
                    mapTiles[x, y] = TileType.Floor;
                }
            }
        }
    }

    private void ConnectRooms()
    {
        if (rooms.Count < 2) return;

        List<Room> unconnectedRooms = new List<Room>(rooms);
        List<Room> connectedRooms = new List<Room>();

        Room startRoom = rooms[0];
        startRoom.isConnected = true;
        connectedRooms.Add(startRoom);
        unconnectedRooms.Remove(startRoom);

        while (unconnectedRooms.Count > 0)
        {
            float closestDistance = float.MaxValue;
            Room closestConnected = null;
            Room closestUnconnected = null;

            foreach (Room connected in connectedRooms)
            {
                foreach (Room unconnected in unconnectedRooms)
                {
                    float distance = Vector2.Distance(connected.Center, unconnected.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestConnected = connected;
                        closestUnconnected = unconnected;
                    }
                }
            }

            if (closestUnconnected != null)
            {
                closestUnconnected.isConnected = true;
                connectedRooms.Add(closestUnconnected);
                unconnectedRooms.Remove(closestUnconnected);

                closestConnected.connectedRooms.Add(closestUnconnected);
                closestUnconnected.connectedRooms.Add(closestConnected);
            }
        }
    }

    private void GenerateCorridors()
    {
        foreach (Room room in rooms)
        {
            foreach (Room connectedRoom in room.connectedRooms)
            {
                CreateCorridor(room, connectedRoom);
            }
        }
    }

    private void CreateCorridor(Room roomA, Room roomB)
    {
        Corridor corridor = new Corridor();

        int startX = Mathf.RoundToInt(roomA.Center.x);
        int startY = Mathf.RoundToInt(roomA.Center.y);
        int endX = Mathf.RoundToInt(roomB.Center.x);
        int endY = Mathf.RoundToInt(roomB.Center.y);

        int currentX = startX;
        int currentY = startY;

        bool horizontalFirst = random.Next(0, 2) == 0;

        if (horizontalFirst)
        {
            while (currentX != endX)
            {
                CarveCorridorTile(currentX, currentY, corridor);
                currentX += currentX < endX ? 1 : -1;
            }
            while (currentY != endY)
            {
                CarveCorridorTile(currentX, currentY, corridor);
                currentY += currentY < endY ? 1 : -1;
            }
        }
        else
        {
            while (currentY != endY)
            {
                CarveCorridorTile(currentX, currentY, corridor);
                currentY += currentY < endY ? 1 : -1;
            }
            while (currentX != endX)
            {
                CarveCorridorTile(currentX, currentY, corridor);
                currentX += currentX < endX ? 1 : -1;
            }
        }

        CarveCorridorTile(endX, endY, corridor);
        corridors.Add(corridor);
    }

    private void CarveCorridorTile(int x, int y, Corridor corridor)
    {
        for (int dx = -corridorWidth; dx <= corridorWidth; dx++)
        {
            for (int dy = -corridorWidth; dy <= corridorWidth; dy++)
            {
                int tileX = x + dx;
                int tileY = y + dy;

                if (tileX >= 0 && tileX < mapWidth && tileY >= 0 && tileY < mapHeight)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) <= corridorWidth)
                    {
                        mapTiles[tileX, tileY] = TileType.Floor;
                        corridor.tiles.Add(new Vector2Int(tileX, tileY));
                    }
                }
            }
        }
    }

    private void GenerateWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapTiles[x, y] == TileType.Floor)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int checkX = x + dx;
                            int checkY = y + dy;

                            if (checkX >= 0 && checkX < mapWidth &&
                                checkY >= 0 && checkY < mapHeight)
                            {
                                if (mapTiles[checkX, checkY] == TileType.Wall)
                                {
                                    mapTiles[checkX, checkY] = TileType.Wall;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void GenerateObstacles()
    {
        foreach (Room room in rooms)
        {
            int obstacleCount = Mathf.FloorToInt(room.width * room.height * obstacleDensity / 100f);

            for (int i = 0; i < obstacleCount; i++)
            {
                int obstacleX = random.Next(room.x + 1, room.x + room.width - 1);
                int obstacleY = random.Next(room.y + 1, room.y + room.height - 1);

                if (mapTiles[obstacleX, obstacleY] == TileType.Floor)
                {
                    mapTiles[obstacleX, obstacleY] = TileType.Obstacle;
                }
            }
        }
    }

    private void GenerateCollectibles()
    {
        foreach (Room room in rooms)
        {
            int collectibleCount = Mathf.FloorToInt(room.width * room.height * collectibleDensity / 100f);
            collectibleCount = Mathf.Max(1, collectibleCount);

            for (int i = 0; i < collectibleCount; i++)
            {
                int attempts = 0;
                while (attempts < 20)
                {
                    attempts++;
                    int collectX = random.Next(room.x + 1, room.x + room.width - 1);
                    int collectY = random.Next(room.y + 1, room.y + room.height - 1);

                    if (mapTiles[collectX, collectY] == TileType.Floor)
                    {
                        mapTiles[collectX, collectY] = TileType.Collectible;
                        break;
                    }
                }
            }
        }
    }

    private void GenerateOreNodes()
    {
        foreach (Room room in rooms)
        {
            int oreCount = Mathf.FloorToInt(room.width * room.height * oreNodeDensity / 100f);
            oreCount = Mathf.Max(0, oreCount);

            for (int i = 0; i < oreCount; i++)
            {
                int attempts = 0;
                while (attempts < 20)
                {
                    attempts++;
                    int oreX = random.Next(room.x + 1, room.x + room.width - 1);
                    int oreY = random.Next(room.y + 1, room.y + room.height - 1);

                    if (mapTiles[oreX, oreY] == TileType.Floor)
                    {
                        mapTiles[oreX, oreY] = TileType.OreNode;
                        break;
                    }
                }
            }
        }
    }

    private void SetStartAndExitPoints()
    {
        if (rooms.Count < 2) return;

        Room startRoom = rooms[0];
        Room exitRoom = rooms[rooms.Count - 1];

        int startX = Mathf.RoundToInt(startRoom.Center.x);
        int startY = Mathf.RoundToInt(startRoom.Center.y);
        mapTiles[startX, startY] = TileType.StartPoint;
        StartPosition = TileToWorldPosition(startX, startY);

        int exitX = Mathf.RoundToInt(exitRoom.Center.x);
        int exitY = Mathf.RoundToInt(exitRoom.Center.y);
        mapTiles[exitX, exitY] = TileType.ExitPoint;
        ExitPosition = TileToWorldPosition(exitX, exitY);
    }

    private void BuildMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                TileType tileType = mapTiles[x, y];
                Vector3 worldPos = TileToWorldPosition(x, y);

                switch (tileType)
                {
                    case TileType.Floor:
                        CreateFloorTile(worldPos);
                        break;
                    case TileType.Wall:
                        CreateWallTile(worldPos);
                        break;
                    case TileType.Obstacle:
                        CreateFloorTile(worldPos);
                        CreateObstacleTile(worldPos);
                        break;
                    case TileType.Collectible:
                        CreateFloorTile(worldPos);
                        CreateCollectible(worldPos);
                        break;
                    case TileType.OreNode:
                        CreateFloorTile(worldPos);
                        CreateOreNode(worldPos);
                        break;
                    case TileType.StartPoint:
                    case TileType.ExitPoint:
                        CreateFloorTile(worldPos);
                        break;
                }
            }
        }

        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, ExitPosition, Quaternion.identity, mapParent);
        }

        if (bakeNavMesh && navMeshGenerator != null)
        {
            Invoke(nameof(BakeCaveNavMesh), navMeshBakeDelay);
        }
    }

    private void BakeCaveNavMesh()
    {
        navMeshGenerator?.BakeNavMeshImmediate();
    }

    private void CreateFloorTile(Vector3 position)
    {
        if (floorPrefab != null)
        {
            Instantiate(floorPrefab, position, Quaternion.identity, mapParent);
        }
        else
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = position;
            floor.transform.localScale = new Vector3(tileSize * 0.95f, 0.2f, tileSize * 0.95f);
            floor.GetComponent<Renderer>().material.color = new Color(0.3f, 0.25f, 0.2f);
            floor.transform.SetParent(mapParent);
        }
    }

    private void CreateWallTile(Vector3 position)
    {
        if (wallPrefab != null)
        {
            Instantiate(wallPrefab, position + Vector3.up * wallHeight / 2f, Quaternion.identity, mapParent);
        }
        else
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.position = position + Vector3.up * wallHeight / 2f;
            wall.transform.localScale = new Vector3(tileSize * 0.95f, wallHeight, tileSize * 0.95f);
            wall.GetComponent<Renderer>().material.color = new Color(0.2f, 0.18f, 0.15f);
            wall.transform.SetParent(mapParent);
        }
    }

    private void CreateObstacleTile(Vector3 position)
    {
        if (obstaclePrefab != null)
        {
            Instantiate(obstaclePrefab, position + Vector3.up * 0.5f, Quaternion.identity, mapParent);
        }
        else
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obstacle.name = "Obstacle";
            obstacle.transform.position = position + Vector3.up * 0.5f;
            obstacle.transform.localScale = new Vector3(tileSize * 0.4f, 1f, tileSize * 0.4f);
            obstacle.GetComponent<Renderer>().material.color = new Color(0.4f, 0.35f, 0.3f);
            obstacle.transform.SetParent(mapParent);
        }
    }

    private void CreateCollectible(Vector3 position)
    {
        if (collectiblePrefabs != null && collectiblePrefabs.Length > 0)
        {
            int prefabIndex = random.Next(0, collectiblePrefabs.Length);
            Instantiate(collectiblePrefabs[prefabIndex], position + Vector3.up * 1f, Quaternion.identity, mapParent);
        }
        else
        {
            GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collectible.name = "Collectible";
            collectible.transform.position = position + Vector3.up * 1f;
            collectible.transform.localScale = Vector3.one * 0.5f;
            collectible.GetComponent<Renderer>().material.color = Color.Lerp(Color.yellow, Color.green, (float)random.NextDouble());
            collectible.transform.SetParent(mapParent);

            CollectibleResource resource = collectible.AddComponent<CollectibleResource>();
        }
    }

    private void CreateOreNode(Vector3 position)
    {
        OreNodeType[] oreTypes = { OreNodeType.Iron, OreNodeType.Copper, OreNodeType.Silver, OreNodeType.Gold, OreNodeType.Crystal, OreNodeType.Relic };
        float[] weights = { 0.35f, 0.25f, 0.18f, 0.10f, 0.08f, 0.04f };

        float randomValue = (float)random.NextDouble();
        float cumulativeWeight = 0f;
        OreNodeType selectedType = OreNodeType.Iron;

        for (int i = 0; i < oreTypes.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                selectedType = oreTypes[i];
                break;
            }
        }

        GameObject oreNode = new GameObject($"OreNode_{selectedType}");
        oreNode.transform.position = position + Vector3.up * 0.5f;
        oreNode.transform.SetParent(mapParent);

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(oreNode.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

        Color oreColor = GetOreColor(selectedType);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.color = oreColor;
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", oreColor * 0.2f);

        Destroy(visual.GetComponent<Collider>());

        BoxCollider collider = oreNode.AddComponent<BoxCollider>();
        collider.size = new Vector3(1f, 1.5f, 1f);
        collider.center = new Vector3(0, 0.5f, 0);

        DestructibleOreNode ore = oreNode.AddComponent<DestructibleOreNode>();
    }

    private Color GetOreColor(OreNodeType type)
    {
        return type switch
        {
            OreNodeType.Iron => new Color(0.7f, 0.5f, 0.3f),
            OreNodeType.Copper => new Color(0.8f, 0.5f, 0.2f),
            OreNodeType.Silver => new Color(0.9f, 0.9f, 0.95f),
            OreNodeType.Gold => new Color(1f, 0.85f, 0f),
            OreNodeType.Crystal => new Color(0.8f, 0.3f, 1f),
            OreNodeType.Relic => new Color(0.3f, 0.8f, 0.8f),
            _ => Color.gray
        };
    }

    public Vector3 TileToWorldPosition(int tileX, int tileY)
    {
        return new Vector3(
            (tileX - mapWidth / 2f) * tileSize,
            0f,
            (tileY - mapHeight / 2f) * tileSize
        );
    }

    public Vector2Int WorldToTilePosition(Vector3 worldPosition)
    {
        int tileX = Mathf.RoundToInt(worldPosition.x / tileSize + mapWidth / 2f);
        int tileY = Mathf.RoundToInt(worldPosition.z / tileSize + mapHeight / 2f);
        return new Vector2Int(tileX, tileY);
    }

    public void RegenerateCave()
    {
        if (useRandomSeed)
        {
            seed = System.DateTime.Now.GetHashCode();
        }
        random = new System.Random(seed);
        GenerateCave();
    }

    private void OnDrawGizmosSelected()
    {
        if (rooms == null) return;

        Gizmos.color = Color.green;
        foreach (Room room in rooms)
        {
            Vector3 min = TileToWorldPosition(room.x, room.y);
            Vector3 max = TileToWorldPosition(room.x + room.width, room.y + room.height);
            Vector3 size = new Vector3(max.x - min.x, 1f, max.z - min.z);
            Vector3 center = (min + max) / 2f;
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = Color.blue;
        foreach (Corridor corridor in corridors)
        {
            foreach (Vector2Int tile in corridor.tiles)
            {
                Gizmos.DrawWireCube(TileToWorldPosition(tile.x, tile.y) + Vector3.up * 0.5f, Vector3.one * tileSize * 0.8f);
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(StartPosition + Vector3.up * 1f, 1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(ExitPosition + Vector3.up * 1f, 1f);
    }
}
