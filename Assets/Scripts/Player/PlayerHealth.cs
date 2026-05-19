using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private float invincibilityDuration = 0.5f;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtPushForce = 5f;
    [SerializeField] private float hurtDuration = 0.3f;

    private bool isInvincible;
    private float invincibilityTimer;
    private bool isHurt;
    private float hurtTimer;

    private PlayerController playerController;
    private CharacterController controller;

    public bool IsDead => currentHealth <= 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsHurt => isHurt;
    public bool IsInvincible => isInvincible;

    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnHurt;
    public System.Action OnHeal;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        UpdateInvincibility();
        UpdateHurtState();
    }

    private void UpdateInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
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
            }
        }
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (IsDead || isInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ApplyHurtEffect(hitDirection);
        }

        OnHurt?.Invoke();
        StartInvincibility();
    }

    private void ApplyHurtEffect(Vector3 hitDirection)
    {
        isHurt = true;
        hurtTimer = hurtDuration;

        if (controller != null)
        {
            Vector3 pushDirection = hitDirection.normalized;
            pushDirection.y = 0.2f;
            controller.Move(pushDirection * hurtPushForce * 0.1f);
        }
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (currentHealth > previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth);
            OnHeal?.Invoke();
        }
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        OnHeal?.Invoke();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("玩家死亡！");
    }

    public void SetMaxHealth(int newMaxHealth, bool healToFull = true)
    {
        maxHealth = newMaxHealth;
        if (healToFull)
        {
            FullHeal();
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        isHurt = false;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
