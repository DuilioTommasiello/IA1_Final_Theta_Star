using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_HealState : State
{
    public Minion_HealState(Agent agent) : base(agent) { }

    protected override void OnEnter()
    {
        if (agent is Minion minion)
        {

        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (agent is Minion minion)
        {

        }
    }

    protected override void OnExit()
    {
        if (agent is Minion minion)
        {

        }
    }
}
