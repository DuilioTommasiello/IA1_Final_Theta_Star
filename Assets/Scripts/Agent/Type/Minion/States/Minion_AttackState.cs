using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_AttackState : State
{
    private Minion minion;
    private Agent targetEnemy;
    private float checkInterval = 0.3f;
    private float timer = 0f;
    private float originalMaxSpeed;

    public Minion_AttackState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log($"{minion.name}: Entering Attack State");

        // Guardar la velocidad original
        originalMaxSpeed = minion._maxSpeed;

        // Buscar el enemigo más cercano que NO esté en estado Flee
        targetEnemy = minion.FindClosestEnemyExcludingFlee();
        minion.CurrentTargetEnemy = targetEnemy;

        if (targetEnemy == null)
        {
            // Si no hay enemigos, volver a Follow
            minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion == null) return;

        timer += deltaTime;

        // Si no tenemos objetivo, buscar uno
        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
        {
            targetEnemy = minion.FindClosestEnemyExcludingFlee();
            minion.CurrentTargetEnemy = targetEnemy;

            if (targetEnemy == null)
            {
                minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
                return;
            }
        }

        // Calcular distancia al enemigo
        float distanceToEnemy = Vector3.Distance(minion.transform.position, targetEnemy.transform.position);

        if (distanceToEnemy > minion.AttackRange)
        {
            // Moverse hacia el enemigo si está fuera de rango
            Vector3 steering = minion.Seek(targetEnemy.transform.position);
            minion.AddForce(steering * 1.1f); // Ligeramente más agresivo

            // Mantener distancia mínima para ataque
            if (distanceToEnemy < minion.AttackRange * 1.2f)
            {
                minion.AddForce(minion.Flee(targetEnemy.transform.position) * 0.3f);
            }
        }
        else
        {
            // Mantener distancia óptima de ataque
            float optimalDistance = minion.AttackRange * 0.8f;
            if (distanceToEnemy < optimalDistance)
            {
                // Retroceder un poco
                Vector3 fleeForce = minion.Flee(targetEnemy.transform.position);
                minion.AddForce(fleeForce * 0.5f);
            }

            // Atacar si puede
            if (minion.CanAttack)
            {
                minion.Attack(targetEnemy);
            }
        }

        // Evitar obstáculos
        if (minion.HasObstacleToAvoid(minion.ObstacleMask))
        {
            Vector3 avoidanceForce = minion.ObstacleAvoidance(minion.ObstacleMask);
            minion.AddForce(avoidanceForce);
        }

        // Verificar transiciones periódicamente
        if (timer >= checkInterval)
        {
            CheckTransitions();
            timer = 0f;
        }
    }

    private void CheckTransitions()
    {
        // Verificar si la vida es baja
        if (minion.IsLowHealth)
        {
            minion.SendInputToFSM(minion.INPUT_LOWHP);
            return;
        }

        // Verificar si el enemigo se perdió de vista
        if (targetEnemy != null)
        {
            float distance = Vector3.Distance(minion.transform.position, targetEnemy.transform.position);

            // Verificar línea de visión
            Vector3 direction = targetEnemy.transform.position - minion.transform.position;
            bool hasLOS = !Physics.Raycast(minion.transform.position, direction.normalized,
                distance, minion.ObstacleMask);

            if (!hasLOS || distance > minion._viewRadius)
            {
                minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
                return;
            }
        }
        else
        {
            // No hay enemigo visible
            minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
        }
    }

    protected override void OnExit()
    {
        Debug.Log($"{minion.name}: Exiting Attack State");

        // Restaurar velocidad original
        minion._maxSpeed = originalMaxSpeed;
        minion.Stop();
        minion.CurrentTargetEnemy = null;
    }
}
