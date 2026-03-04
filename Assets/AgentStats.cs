using UnityEngine;

public class AgentStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float currentHealth;
    [SerializeField, Range(100f, 300f)] private float maxHealth = 100f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;

    [SerializeField] private HealthBar healthBar;

    public System.Action OnDamageTaken;
    public System.Action OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;

        healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Update()
    {
        healthBar.UpdateHealBar(maxHealth, currentHealth);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}