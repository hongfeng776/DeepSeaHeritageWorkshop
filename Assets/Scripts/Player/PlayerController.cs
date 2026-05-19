using UnityEngine;
using System.Collections.Generic;

public enum PlayerState
{
    Idle,
    Walking,
    Jumping,
    Falling,
    Attacking,
    Hurt,
    Dead
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float airControl = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float jumpCooldown = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float attackHitTime = 0.15f;
    [SerializeField] private Vector3 attackBoxSize = new Vector3(1.5f, 2f, 1f);
    [SerializeField] private LayerMask attackLayerMask;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraFollowTarget;
    [SerializeField] private float cameraDistance = 6f;
    [SerializeField] private float cameraHeight = 2f;
    [SerializeField] private float cameraRotationSpeed = 3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Collision Settings")]
    [SerializeField] private float collisionRadius = 0.5f;
    [SerializeField] private float collisionHeight = 2f;
    [SerializeField] private LayerMask collisionLayerMask;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private bool isGrounded;
    private int currentJumpCount;
    private float lastJumpTime;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpRequested;

    private float lastAttackTime;
    private bool isAttacking;
    private float attackTimer;
    private bool hasHitThisAttack;
    private List<GameObject> hitObjectsThisAttack;

    private float cameraYaw = 0f;
    private float cameraPitch = 30f;
    private Transform mainCamera;

    private PlayerState currentState;
    private PlayerState previousState;

    public Vector3 MovementDirection => moveDirection;
    public bool IsMoving => moveDirection.magnitude > 0.1f;
    public float CurrentSpeed { get; private set; }
    public bool IsGrounded => isGrounded;
    public bool IsAttacking => isAttacking;
    public PlayerState CurrentState => currentState;

    public System.Action<PlayerState> OnStateChanged;
    public System.Action OnJump;
    public System.Action OnAttackStart;
    public System.Action<GameObject> OnAttackHitObject;
    public System.Action OnLand;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        hitObjectsThisAttack = new List<GameObject>();

        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        currentJumpCount = maxJumpCount;

        if (attackLayerMask == 0)
        {
            attackLayerMask = LayerMask.GetMask("Default", "Enemy");
        }

        if (groundMask == 0)
        {
            groundMask = LayerMask.GetMask("Default", "Ground");
        }

        if (collisionLayerMask == 0)
        {
            collisionLayerMask = LayerMask.GetMask("Default", "Ground", "Enemy");
        }
    }

    private void Start()
    {
        if (cameraFollowTarget == null)
        {
            CreateCameraFollowTarget();
        }

        if (groundCheck == null)
        {
            CreateGroundCheck();
        }

        SetState(PlayerState.Idle);
    }

    private void CreateCameraFollowTarget()
    {
        GameObject targetObj = new GameObject("CameraFollowTarget");
        targetObj.transform.SetParent(transform);
        targetObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        cameraFollowTarget = targetObj.transform;
    }

    private void CreateGroundCheck()
    {
        GameObject groundObj = new GameObject("GroundCheck");
        groundObj.transform.SetParent(transform);
        groundObj.transform.localPosition = new Vector3(0, 0.1f, 0);
        groundCheck = groundObj.transform;
    }

    private void Update()
    {
        if (currentState == PlayerState.Dead) return;

        HandleTimers();
        HandleGroundCheck();
        HandleInput();
        HandleMovement();
        HandleGravity();
        HandleAttack();
        UpdateState();
    }

    private void LateUpdate()
    {
        HandleCameraRotation();
    }

    private void HandleTimers()
    {
        coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleGroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            velocity.y = Mathf.Max(velocity.y, -2f);

            if (!wasGrounded && velocity.y < 0)
            {
                currentJumpCount = maxJumpCount;
                OnLand?.Invoke();
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
            jumpRequested = true;
        }

        if (jumpBufferCounter > 0f)
        {
            TryJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private void TryJump()
    {
        if (Time.time - lastJumpTime < jumpCooldown) return;

        bool canCoyoteJump = coyoteTimeCounter > 0f;
        bool canAirJump = !isGrounded && currentJumpCount > 0;

        if (canCoyoteJump || canAirJump)
        {
            Jump();
        }
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        currentJumpCount--;
        lastJumpTime = Time.time;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpRequested = false;
        SetState(PlayerState.Jumping);
        OnJump?.Invoke();
    }

    private void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        StartAttack();
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        lastAttackTime = Time.time;
        hasHitThisAttack = false;
        hitObjectsThisAttack.Clear();
        SetState(PlayerState.Attacking);
        OnAttackStart?.Invoke();
    }

    private void HandleAttack()
    {
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            float attackProgress = 1f - (attackTimer / attackDuration);
            if (attackProgress >= (attackHitTime / attackDuration) && !hasHitThisAttack)
            {
                PerformAttackHit();
            }

            if (attackTimer <= 0)
            {
                isAttacking = false;
                hasHitThisAttack = false;
                hitObjectsThisAttack.Clear();
            }
        }
    }

    private void PerformAttackHit()
    {
        hasHitThisAttack = true;

        Vector3 attackOrigin = transform.position + Vector3.up * 1f + transform.forward * 0.5f;
        Quaternion attackRotation = Quaternion.LookRotation(transform.forward);

        Collider[] hitColliders = Physics.OverlapBox(
            attackOrigin,
            attackBoxSize * 0.5f,
            attackRotation,
            attackLayerMask
        );

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == gameObject) continue;
            if (hitObjectsThisAttack.Contains(collider.gameObject)) continue;

            hitObjectsThisAttack.Add(collider.gameObject);

            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector3 hitDirection = (collider.transform.position - transform.position).normalized;
                damageable.TakeDamage(attackDamage, hitDirection);
                OnAttackHitObject?.Invoke(collider.gameObject);
            }
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 inputDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        float inputMagnitude = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);

        if (inputMagnitude > 0.1f)
        {
            moveDirection = inputDirection;
        }
        else
        {
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, deceleration * Time.deltaTime);
        }

        float currentAcceleration = isGrounded ? acceleration : acceleration * airControl;
        float targetSpeed = moveSpeed * inputMagnitude;

        if (isAttacking)
        {
            targetSpeed *= 0.3f;
        }

        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, currentAcceleration * Time.deltaTime);

        if (moveDirection.magnitude > 0.1f)
        {
            controller.Move(moveDirection * CurrentSpeed * Time.deltaTime);

            if (!isAttacking)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            cameraYaw += Input.GetAxis("Mouse X") * cameraRotationSpeed;
            cameraPitch -= Input.GetAxis("Mouse Y") * cameraRotationSpeed;
            cameraPitch = Mathf.Clamp(cameraPitch, -70f, 80f);
        }

        Quaternion cameraRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 cameraPosition = cameraFollowTarget.position - cameraRotation * Vector3.forward * cameraDistance;
        cameraPosition.y += cameraHeight;

        mainCamera.SetPositionAndRotation(cameraPosition, cameraRotation);
    }

    private void UpdateState()
    {
        if (isAttacking)
        {
            SetState(PlayerState.Attacking);
            return;
        }

        if (!isGrounded)
        {
            SetState(velocity.y > 0 ? PlayerState.Jumping : PlayerState.Falling);
            return;
        }

        SetState(IsMoving ? PlayerState.Walking : PlayerState.Idle);
    }

    private void SetState(PlayerState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void SetPosition(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        velocity = Vector3.zero;
        controller.enabled = true;
    }

    public void ResetJumps()
    {
        currentJumpCount = maxJumpCount;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        Gizmos.color = Color.red;
        Vector3 bottom = transform.position + Vector3.up * collisionRadius;
        Vector3 top = transform.position + Vector3.up * (collisionHeight - collisionRadius);
        DrawWireCapsule(bottom, top, collisionRadius);

        Vector3 attackOrigin = transform.position + Vector3.up * 1f + transform.forward * 0.5f;
        Gizmos.matrix = Matrix4x4.TRS(attackOrigin, Quaternion.LookRotation(transform.forward), Vector3.one);
        Gizmos.color = isAttacking ? Color.magenta : Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawWireCapsule(Vector3 bottom, Vector3 top, float radius)
    {
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
        Gizmos.DrawLine(bottom - Vector3.forward * radius, top - Vector3.forward * radius);
        Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
        Gizmos.DrawLine(bottom - Vector3.right * radius, top - Vector3.right * radius);
    }
}
