using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected Agent agent;

    protected State(Agent agent)
    {
        this.agent = agent;
    }

    Dictionary<string, State> transitions = new Dictionary<string, State>();


    public bool ContainsInput(string input)
    {
        return transitions.ContainsKey(input);
    }
    public State GetState(string input)
    {
        return transitions[input];
    }

    public void AddTransition(string _input, State _state)
    {
        if (!transitions.ContainsKey(_input)) transitions.Add(_input, _state);
        else Debug.LogWarning("Key Already Used");
    }
    public void Enter()
    {
        OnEnter();
    }
    public void Exit()
    {
        OnExit();
    }
    public void Update(float deltaTime)
    {
        OnUpdate(deltaTime);
    }

    protected abstract void OnEnter();
    protected abstract void OnUpdate(float deltaTime);
    protected abstract void OnExit();


}
