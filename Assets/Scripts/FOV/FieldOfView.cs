using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0f, 360f)] public float angle;
    [SerializeField, Range(0.1f, 1f)] private float fovRefreshRate = 0.2f;
    public LayerMask targetMask;
    public LayerMask obstructionMask;

    [SerializeField] private Agent currentVisibleEnemy;

    public Agent CurrentVisibleEnemy => currentVisibleEnemy;

    private void Start()
    {
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(fovRefreshRate);
        while (true)
        {
            yield return wait;
            UpdateVisibleEnemy();
        }
    }

    private void UpdateVisibleEnemy()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);
        Agent closestEnemy = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in rangeChecks)
        {
            Agent agent = hit.GetComponent<Agent>();
            if (agent != null)
            {
                Vector3 directionToTarget = (agent.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, agent.transform.position);
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        if (distanceToTarget < closestDist)
                        {
                            closestDist = distanceToTarget;
                            closestEnemy = agent;
                        }
                    }
                }
            }
        }

        currentVisibleEnemy = closestEnemy;
    }

    // Método de utilidad para obtener el enemigo visible (opcional)
    public Agent GetVisibleEnemy()
    {
        return currentVisibleEnemy;
    }
}