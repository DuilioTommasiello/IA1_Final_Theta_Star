using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_HealState : State
{
    private Minion minion;
    private float healTimer = 0f;
    private float healInterval = 1f;

    public Minion_HealState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log($"{minion.name}: Entering Heal State");
        minion.Stop(); // Dejar de moverse para curarse
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion == null) return;

        // coru de heal
        healTimer += deltaTime;
        if (healTimer >= healInterval)
        {
            minion.Heal(minion.HealRate);
            healTimer = 0f;
        }

        // verificar enemigos cercanos
        if (HasEnemyInSight())
        {
            minion.SendInputToFSM(minion.INPUT_ON_LOS);
        }
    }

    private bool HasEnemyInSight()
    {
        // Misma logica de deteccion que en Follow
        Collider[] hitColliders = Physics.OverlapSphere(minion.transform.position, minion._viewRadius * 0.5f);
        foreach (var hitCollider in hitColliders)
        {
            Agent enemy = hitCollider.GetComponent<Agent>();
            if (enemy != null && enemy != minion)
            {
                return true;
            }
        }
        return false;
    }

    protected override void OnExit()
    {
        Debug.Log($"{minion.name}: Exiting Heal State");
    }
}
