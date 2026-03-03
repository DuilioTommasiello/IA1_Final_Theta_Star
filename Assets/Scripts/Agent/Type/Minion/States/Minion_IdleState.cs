using System.Collections;
using System.Collections.Generic;
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
