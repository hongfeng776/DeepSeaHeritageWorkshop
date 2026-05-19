using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Hurt,
    Dead
}

[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;

    [Header("Combat Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float lostTargetRange = 18f;

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolRange = 6f;
    [SerializeField] private float idleTimeBetweenPatrols = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private float knockbackForce = 4f;

    [Header("References")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Renderer enemyRenderer;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color hurtColor = Color.white;
    [SerializeField] private Color chaseColor = Color.magenta;

    [Header("Loot Settings")]
    [SerializeField] private bool dropLootOnDeath = true;
    [SerializeField] private int minGoldDrop = 5;
    [SerializeField] private int maxGoldDrop = 20;
    [SerializeField] private float resourceDropChance = 0.7f;
    [SerializeField] private ResourceType[] possibleDrops = { ResourceType.IronOre, ResourceType.Crystal, ResourceType.Energy };

    private NavMeshAgent agent;
    private Transform player;
    private EnemyState currentState;
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private float lastAttackTime;
    private bool isHurt;
    private float hurtTimer;
    private float idleTimer;
    private bool hasValidPatrolTarget;

    public bool IsDead => currentHealth <= 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public EnemyState CurrentState => currentState;

    public System.Action OnDeath;
    public System.Action OnHurt;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        startPosition = transform.position;

        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }
    }

    private void Start()
    {
        FindPlayer();
        SetState(EnemyState.Idle);
        idleTimer = idleTimeBetweenPatrols;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (IsDead) return;

        UpdateHurtState();

        if (!isHurt)
        {
            UpdateStateMachine();
        }
    }

    private void UpdateHurtState()
    {
        if (isHurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                isHurt = false;
                UpdateEnemyColor();
            }
        }
    }

    private void UpdateStateMachine()
    {
        float distanceToPlayer = GetDistanceToPlayer();
        bool canSeePlayer = CanSeePlayer();

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer, canSeePlayer);
                break;

            case EnemyState.Patrol:
                HandlePatrolState(distanceToPlayer, canSeePlayer);
                break;

            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer, canSeePlayer);
                break;

            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer, canSeePlayer);
                break;
        }
    }

    private void HandleIdleState(float distanceToPlayer, bool canSeePlayer)
    {
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            SetState(EnemyState.Chase);
            return;
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            SetState(EnemyState.Patrol);
            SetNewPatrolTarget();
        }
    }

    private void HandlePatrolState(float distanceToPlayer, bool canSeePlayer)
    {
        if (canSeePlayer && distanceToPlayer <= detectionRange)
        {
            SetState(EnemyState.Chase);
            return;
        }

        if (!hasValidPatrolTarget)
        {
            SetState(EnemyState.Idle);
            idleTimer = idleTimeBetweenPatrols;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, patrolTarget);
        if (distanceToTarget < 1.5f)
        {
            SetState(EnemyState.Idle);
            idleTimer = idleTimeBetweenPatrols;
            hasValidPatrolTarget = false;
        }
    }

    private void HandleChaseState(float distanceToPlayer, bool canSeePlayer)
    {
        if (!canSeePlayer && distanceToPlayer > lostTargetRange)
        {
            SetState(EnemyState.Patrol);
            SetNewPatrolTarget();
            return;
        }

        if (distanceToPlayer <= attackRange && CanSeePlayer())
        {
            SetState(EnemyState.Attack);
            return;
        }

        if (player != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    private void HandleAttackState(float distanceToPlayer, bool canSeePlayer)
    {
        agent.velocity = Vector3.zero;

        if (player != null)
        {
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        if (distanceToPlayer > attackRange * 1.2f)
        {
            SetState(EnemyState.Chase);
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown && canSeePlayer)
        {
            PerformAttack();
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * patrolRange;
        patrolTarget = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(patrolTarget, out hit, patrolRange, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            hasValidPatrolTarget = true;
            
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(patrolTarget);
            }
        }
        else
        {
            hasValidPatrolTarget = false;
        }
    }

    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Idle:
                agent.speed = 0;
                break;

            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                break;

            case EnemyState.Attack:
                agent.speed = 0;
                break;
        }

        UpdateEnemyColor();
    }

    private void UpdateEnemyColor()
    {
        if (enemyRenderer != null && !isHurt)
        {
            switch (currentState)
            {
                case EnemyState.Chase:
                case EnemyState.Attack:
                    enemyRenderer.material.color = chaseColor;
                    break;
                default:
                    enemyRenderer.material.color = normalColor;
                    break;
            }
        }
    }

    private float GetDistanceToPlayer()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return float.MaxValue;
        }
        return Vector3.Distance(transform.position, player.position);
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 startPos = transform.position + Vector3.up * 1f;
        Vector3 direction = (player.position + Vector3.up * 1f - startPos).normalized;
        float distance = Vector3.Distance(startPos, player.position + Vector3.up * 1f);

        if (Physics.Raycast(startPos, direction, distance, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        if (player != null)
        {
            IDamageable playerDamageable = player.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= attackRange)
                {
                    Vector3 hitDirection = (player.position - transform.position).normalized;
                    playerDamageable.TakeDamage(attackDamage, hitDirection);
                }
            }
        }
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHurt?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ApplyKnockback(hitDirection);
        }
    }

    private void ApplyKnockback(Vector3 hitDirection)
    {
        isHurt = true;
        hurtTimer = hurtDuration;

        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = hurtColor;
        }

        if (agent.isOnNavMesh)
        {
            agent.velocity = hitDirection.normalized * knockbackForce;
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void Die()
    {
        SetState(EnemyState.Dead);
        OnDeath?.Invoke();

        if (agent != null)
        {
            agent.enabled = false;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.gray;
        }

        if (dropLootOnDeath)
        {
            DropLoot();
        }

        Destroy(gameObject, 2f);
    }

    private void DropLoot()
    {
        int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);
        DropResourceManager.Instance?.SpawnDrop(ResourceType.Gold, goldAmount, transform.position + Vector3.up * 0.5f, 2f, 0.5f);

        if (Random.value <= resourceDropChance && possibleDrops != null && possibleDrops.Length > 0)
        {
            ResourceType dropType = possibleDrops[Random.Range(0, possibleDrops.Length)];
            int dropAmount = Random.Range(1, 4);
            DropResourceManager.Instance?.SpawnDrop(dropType, dropAmount, transform.position + Vector3.up * 0.5f, 2.5f, 0.6f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, patrolRange);

        if (hasValidPatrolTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, patrolTarget);
        }
    }
}
