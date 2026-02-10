using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_ChaseState : State
{
    private Minion minion;
    private Agent targetEnemy;
    private Vector3 lastKnownPosition;
    private float originalMaxSpeed;
    private float searchTimer = 0f;
    private float searchInterval = 0.5f;
    private float chaseTime = 0f;
    private float maxChaseTime = 10f;

    public Minion_ChaseState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log($"{minion.name}: Entering Chase State");

        // Aumentar velocidad para perseguir
        originalMaxSpeed = minion._maxSpeed;
        minion._maxSpeed *= minion.ChaseSpeedMultiplier;

        // Inicializar temporizador
        chaseTime = 0f;

        // Buscar enemigo para perseguir
        FindEnemyToChase();
    }

    private void FindEnemyToChase()
    {
        // Buscar cualquier enemigo (incluyendo los que están en Flee)
        targetEnemy = minion.FindClosestEnemy();

        if (targetEnemy != null)
        {
            lastKnownPosition = targetEnemy.transform.position;
            minion.CurrentTargetEnemy = targetEnemy;
            Debug.Log($"{minion.name}: Chasing {targetEnemy.name}");
        }
        else
        {
            // Si no hay enemigos, volver a Follow
            minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion == null) return;

        chaseTime += deltaTime;
        searchTimer += deltaTime;

        // Verificar tiempo máximo de persecución
        if (chaseTime >= maxChaseTime)
        {
            minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
            return;
        }

        // Buscar enemigo periódicamente
        if (searchTimer >= searchInterval)
        {
            // Verificar si el objetivo actual sigue siendo válido
            if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
            {
                FindEnemyToChase();
            }
            else
            {
                // Actualizar última posición conocida
                lastKnownPosition = targetEnemy.transform.position;
            }

            searchTimer = 0f;
        }

        // Perseguir al enemigo
        if (targetEnemy != null)
        {
            // Aquí deberías usar Pathfinding Theta*
            // Por ahora, usaremos Seek básico
            Vector3 chaseForce = minion.Seek(lastKnownPosition);
            minion.AddForce(chaseForce * 1.1f);

            // Verificar si el enemigo está ahora en línea de visión
            float distance = Vector3.Distance(minion.transform.position, targetEnemy.transform.position);
            if (distance <= minion._viewRadius)
            {
                Vector3 direction = targetEnemy.transform.position - minion.transform.position;
                bool hasLOS = !Physics.Raycast(minion.transform.position, direction.normalized,
                    distance, minion.ObstacleMask);

                if (hasLOS)
                {
                    minion.SendInputToFSM(minion.INPUT_ON_LOS);
                    return;
                }
            }
        }
        else
        {
            // Ir a la última posición conocida
            Vector3 toLastPosition = lastKnownPosition - minion.transform.position;
            if (toLastPosition.magnitude > minion._stopDistance)
            {
                Vector3 seekForce = minion.Seek(lastKnownPosition);
                minion.AddForce(seekForce);
            }
            else
            {
                // Llegó a la última posición conocida
                minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
                return;
            }
        }

        // Evitar obstáculos
        if (minion.HasObstacleToAvoid(minion.ObstacleMask))
        {
            Vector3 avoidanceForce = minion.ObstacleAvoidance(minion.ObstacleMask);
            minion.AddForce(avoidanceForce);
        }

        // Verificar si la vida es baja
        if (minion.IsLowHealth)
        {
            minion.SendInputToFSM(minion.INPUT_LOWHP);
            return;
        }
    }

    protected override void OnExit()
    {
        Debug.Log($"{minion.name}: Exiting Chase State");

        // Restaurar velocidad original
        minion._maxSpeed = originalMaxSpeed;
        minion.Stop();
        minion.CurrentTargetEnemy = null;
    }
}
