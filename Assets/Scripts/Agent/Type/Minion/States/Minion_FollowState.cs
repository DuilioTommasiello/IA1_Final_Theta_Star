using UnityEngine;

public class Minion_FollowState : State
{
    public Minion minion;

    public Minion_FollowState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUpdate(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnExit()
    {
        throw new System.NotImplementedException();
    }
}