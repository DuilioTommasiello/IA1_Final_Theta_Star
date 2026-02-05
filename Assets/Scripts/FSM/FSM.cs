using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM
{
    State current;

    public State currentState => current;

    public FSM(State initialState)
    {
        current = initialState;
        current.Enter();
    }

    public void SendInput(string input)
    {
        if (current == null) return;

        if (current.ContainsInput(input))
        {
            State next = current.GetState(input);

            if (next == null) throw new System.Exception("No hay estado");
            current.Exit();
            current = next;
            current.Enter();
        }
    }

    public void Update(float delta)
    {
        if (current != null)
        {
            current.Update(delta);
        }
    }
}
