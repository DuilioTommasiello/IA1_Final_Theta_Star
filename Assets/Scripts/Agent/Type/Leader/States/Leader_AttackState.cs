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

        if (leader.CanAttack) leader.Attack(leader.CurrentTargetEnemy);
    }

    protected override void OnExit()
    {
        Debug.Log("Leader Attack State Exit");
    }
}