using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int minEnemiesPerRoom = 1;
    [SerializeField] private int maxEnemiesPerRoom = 3;
    [SerializeField] private float spawnDelay = 0.5f;
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("References")]
    [SerializeField] private CaveGenerator caveGenerator;
    [SerializeField] private Transform playerTransform;

    private List<GameObject> spawnedEnemies;
    private bool isSpawning;

    public List<GameObject> SpawnedEnemies => spawnedEnemies;
    public int TotalSpawned => spawnedEnemies.Count;

    private void Awake()
    {
        spawnedEnemies = new List<GameObject>();

        if (caveGenerator == null)
        {
            caveGenerator = FindObjectOfType<CaveGenerator>();
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    public void StartSpawningEnemies()
    {
        if (isSpawning) return;
        if (caveGenerator == null || caveGenerator.Rooms == null || caveGenerator.Rooms.Count == 0)
        {
            Debug.LogWarning("无法生成敌人：洞穴生成器未准备好或没有房间");
            return;
        }

        isSpawning = true;
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    private System.Collections.IEnumerator SpawnEnemiesCoroutine()
    {
        ClearExistingEnemies();

        List<Room> rooms = caveGenerator.Rooms;
        
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            
            if (i == 0)
            {
                continue;
            }

            int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

            for (int j = 0; j < enemyCount; j++)
            {
                if (TryGetValidSpawnPosition(room, out Vector3 spawnPosition))
                {
                    SpawnEnemy(spawnPosition);
                }

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        isSpawning = false;
        Debug.Log($"敌人生成完成！共生成 {spawnedEnemies.Count} 个敌人");
    }

    private bool TryGetValidSpawnPosition(Room room, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        int attempts = 0;
        int maxAttempts = 20;

        while (attempts < maxAttempts)
        {
            attempts++;

            int tileX = Random.Range(room.x + 1, room.x + room.width - 1);
            int tileY = Random.Range(room.y + 1, room.y + room.height - 1);

            Vector3 worldPos = caveGenerator.TileToWorldPosition(tileX, tileY);
            worldPos.y = 1f;

            if (IsValidSpawnPosition(worldPos))
            {
                spawnPosition = worldPos;
                return true;
            }
        }

        return false;
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(position, playerTransform.position);
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                return false;
            }
        }

        if (Physics.CheckSphere(position, 0.8f, obstacleLayerMask))
        {
            return false;
        }

        return true;
    }

    private void SpawnEnemy(Vector3 position)
    {
        GameObject enemy = CreateEnemy(position);
        if (enemy == null) return;

        spawnedEnemies.Add(enemy);

        BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
        if (baseEnemy != null)
        {
            baseEnemy.OnDeath += () => OnEnemyDied(enemy);
        }
    }

    private GameObject CreateEnemy(Vector3 position)
    {
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (enemyPrefab != null)
            {
                return Instantiate(enemyPrefab, position, Quaternion.identity, transform);
            }
        }

        return EnemyFactory.CreateRandomEnemy(position, transform);
    }

    private void OnEnemyDied(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
        }
    }

    public void ClearExistingEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    public void SetSpawnParameters(int minPerRoom, int maxPerRoom)
    {
        minEnemiesPerRoom = minPerRoom;
        maxEnemiesPerRoom = maxPerRoom;
    }

    private void OnDrawGizmosSelected()
    {
        if (caveGenerator == null || caveGenerator.Rooms == null) return;

        Gizmos.color = Color.red;
        foreach (Room room in caveGenerator.Rooms)
        {
            Vector3 center = caveGenerator.TileToWorldPosition(
                Mathf.RoundToInt(room.Center.x),
                Mathf.RoundToInt(room.Center.y)
            );
            Gizmos.DrawWireSphere(center + Vector3.up * 1f, 1f);
        }
    }
}
