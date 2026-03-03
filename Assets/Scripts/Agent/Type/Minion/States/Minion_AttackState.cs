using System.Collections;
using System.Collections.Generic;
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

    }

    protected override void OnUpdate(float deltaTime)
    {
        
    }

    protected override void OnExit()
    {

    }
}
