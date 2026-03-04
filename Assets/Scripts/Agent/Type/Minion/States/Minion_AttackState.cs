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
        minion.Stop();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion.CurrentTargetEnemy == null)
        {
            if (minion.Leader != null && Vector3.Distance(minion.transform.position, minion.Leader.transform.position) <= minion.FollowStopDistance)
                minion.SendInput(Minion.INPUT_LOST_TO_IDLE);
            else
                minion.SendInput(Minion.INPUT_LOST_TO_FOLLOW);
            return;
        }

        minion.LookAt(minion.CurrentTargetEnemy.transform.position);

        if (minion.CanAttack)
        {
            minion.Attack(minion.CurrentTargetEnemy);
        }
    }

    protected override void OnExit()
    {

    }
}