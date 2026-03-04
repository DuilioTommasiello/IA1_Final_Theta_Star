using UnityEngine;

public class Minion_AttackState : State
{
    private Minion minion;

    public Minion_AttackState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log("Minion Attack State Enter");
        minion.Stop();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion.CurrentTargetEnemy == null)
        {
            minion.SendInput(Agent.INPUT_ENEMY_LOST);
            return;
        }

        minion.LookAt(minion.CurrentTargetEnemy.transform.position);

        float dist = Vector3.Distance(minion.transform.position, minion.CurrentTargetEnemy.transform.position);
        if (dist <= minion.AttackRange)
        {
            Vector3 dir = (minion.CurrentTargetEnemy.transform.position - minion.transform.position).normalized;
            if (!Physics.Raycast(minion.transform.position, dir, dist, minion.ObstacleMask))
            {
                if (minion.CanAttack)
                {
                    minion.Attack(minion.CurrentTargetEnemy);
                }
            }
        }
    }

    protected override void OnExit()
    {
        Debug.Log("Minion Attack State Exit");
    }
}