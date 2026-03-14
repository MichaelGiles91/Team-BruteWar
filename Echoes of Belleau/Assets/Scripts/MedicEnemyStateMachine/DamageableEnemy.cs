using UnityEngine;

public class DamageableEnemy : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public bool NeedsHealing()
    {
        return currentHealth < maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}
