using UnityEngine;

public class Minion_IdleState : State
{
    private Minion minion;

    public Minion_IdleState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        minion.Stop();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion.Leader == null) return;

        float distToLeader = Vector3.Distance(minion.transform.position, minion.Leader.transform.position);
        if (distToLeader > minion.FollowStopDistance)
        {
            minion.SendInput(Minion.INPUT_TOO_FAR);
        }
    }

    protected override void OnExit()
    {

    }
}