using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_FleeState : State
{
    private Minion minion;
    private Agent closestEnemy;
    private Vector3 fleeDirection;
    private float originalMaxSpeed;
    private float recalcTimer = 0f;
    private float recalcInterval = 1f;

    public Minion_FleeState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log($"{minion.name}: Entering Flee State");

        // Aumentar velocidad para huir
        originalMaxSpeed = minion._maxSpeed;
        minion._maxSpeed *= minion.FleeSpeedMultiplier;

        // Calcular posición segura inicial
        minion.SetSafePosition(minion.CalculateSafeFleePosition());

        // Buscar enemigo más cercano
        closestEnemy = minion.FindClosestEnemy();

        if (closestEnemy != null)
        {
            // Calcular dirección de huida (opuesta al enemigo)
            fleeDirection = (minion.transform.position - closestEnemy.transform.position).normalized;
            fleeDirection.y = 0;
        }
        else
        {
            // Si no hay enemigos, huir en dirección aleatoria
            fleeDirection = Random.onUnitSphere;
            fleeDirection.y = 0;
            fleeDirection.Normalize();
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion == null) return;

        recalcTimer += deltaTime;

        // Recalcular periódicamente
        if (recalcTimer >= recalcInterval)
        {
            closestEnemy = minion.FindClosestEnemy();
            recalcTimer = 0f;
        }

        // Huir del enemigo más cercano
        if (closestEnemy != null)
        {
            // Calcular dirección de huida
            fleeDirection = (minion.transform.position - closestEnemy.transform.position).normalized;
            fleeDirection.y = 0;

            // Añadir aleatoriedad para evitar esquinas
            fleeDirection += Random.insideUnitSphere * 0.3f;
            fleeDirection.y = 0;
            fleeDirection.Normalize();

            // Calcular fuerza de huida
            Vector3 fleeForce = minion.Flee(closestEnemy.transform.position);
            minion.AddForce(fleeForce * 1.5f); // Más intenso

            // También buscar la posición segura
            Vector3 toSafePosition = minion.GetSafePosition() - minion.transform.position;
            if (toSafePosition.magnitude > 0.1f)
            {
                toSafePosition.Normalize();
                minion.AddForce(toSafePosition * minion._maxForce * 0.5f);
            }
        }
        else
        {
            // Si no hay enemigos, ir directamente a la posición segura
            Vector3 toSafePosition = minion.GetSafePosition() - minion.transform.position;
            if (toSafePosition.magnitude > minion._stopDistance)
            {
                toSafePosition.Normalize();
                Vector3 seekForce = minion.Seek(minion.GetSafePosition());
                minion.AddForce(seekForce);
            }
        }

        // Evitar obstáculos (prioridad alta al huir)
        if (minion.HasObstacleToAvoid(minion.ObstacleMask))
        {
            Vector3 avoidanceForce = minion.ObstacleAvoidance(minion.ObstacleMask);
            minion.AddForce(avoidanceForce * 2f); // Más fuerte al huir
        }

        // Verificar si llegó a posición segura
        if (minion.IsAtSafePosition())
        {
            minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
            return;
        }

        // Verificar si el líder está muerto
        if (minion.target == null || !minion.target.gameObject.activeSelf)
        {
            minion.SendInputToFSM(minion.INPUT_LEADERISDEAD);
            return;
        }

        // Si hay enemigos cerca, mantenerse en Flee (esto ya está implícito)
    }

    protected override void OnExit()
    {
        Debug.Log($"{minion.name}: Exiting Flee State");

        // Restaurar velocidad original
        minion._maxSpeed = originalMaxSpeed;
        minion.Stop();
    }
}
