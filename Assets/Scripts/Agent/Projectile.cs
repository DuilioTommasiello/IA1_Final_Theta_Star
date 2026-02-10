using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject hitEffect;

    private Transform target;
    private float damage;
    private float spawnTime;

    public void Initialize(Transform targetTransform, float damageAmount)
    {
        target = targetTransform;
        damage = damageAmount;
        spawnTime = Time.time;

        // Destruir automáticamente después de un tiempo
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target != null && target.gameObject.activeSelf)
        {
            // Moverse hacia el objetivo
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.forward = direction;

            // Verificar colisión
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < 1f)
            {
                OnHit(target);
                Destroy(gameObject);
            }
        }
        else
        {
            // Si el objetivo desaparece, seguir en línea recta
            transform.position += transform.forward * speed * Time.deltaTime;

            // Destruir si ha pasado mucho tiempo
            if (Time.time - spawnTime > lifeTime * 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnHit(Transform hitTarget)
    {
        Agent agent = hitTarget.GetComponent<Agent>();
        if (agent is Minion minion)
        {
            minion.TakeDamage(damage);
        }

        // Efecto de impacto
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        Debug.Log($"Projectile hit {hitTarget.name} for {damage} damage");
    }
}