using UnityEngine;

public class Leader_AttackState : State
{
    private Leader leader;

    public Leader_AttackState(Agent agent) : base(agent)
    {
        leader = agent as Leader;
    }

    protected override void OnEnter()
    {
        Debug.Log("Leader Attack State Enter");
        leader.Stop();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (leader.CurrentTargetEnemy == null)
        {
            leader.SendInput(Agent.INPUT_ENEMY_LOST);
            return;
        }

        leader.LookAt(leader.CurrentTargetEnemy.transform.position);

        float dist = Vector3.Distance(leader.transform.position, leader.CurrentTargetEnemy.transform.position);
        if (dist <= leader.AttackRange)
        {
            Vector3 dir = (leader.CurrentTargetEnemy.transform.position - leader.transform.position).normalized;
            if (!Physics.Raycast(leader.transform.position, dir, dist, leader.ObstacleMask))
            {
                if (leader.CanAttack)
                {
                    leader.Attack(leader.CurrentTargetEnemy);
                }
            }
        }
    }

    protected override void OnExit()
    {
        Debug.Log("Leader Attack State Exit");
    }
}