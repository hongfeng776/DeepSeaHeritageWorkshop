using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, Vector3 hitDirection);
    void Heal(int amount);
    bool IsDead { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
}
