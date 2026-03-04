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
        Debug.Log("Minion Idle State Enter");
        minion.Stop();
    }

    protected override void OnUpdate(float deltaTime)
    {
        // No hace nada, espera a que llegue un input
    }

    protected override void OnExit()
    {
        Debug.Log("Minion Idle State Exit");
    }
}