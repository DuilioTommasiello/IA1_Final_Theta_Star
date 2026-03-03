using System.Collections;
using System.Collections.Generic;
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
