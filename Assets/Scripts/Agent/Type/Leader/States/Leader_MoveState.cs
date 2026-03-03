using UnityEngine;

public class Leader_MoveState : State
{
    private Leader leader;

    public Leader_MoveState(Agent agent) : base(agent)
    {
        leader = agent as Leader;
    }

    protected override void OnEnter()
    {
        Debug.Log("Move");
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (!leader.HasDestination)
        {
            // Si no hay destino, volvemos a idle
            leader.SendInput(leader.INPUT_ARRIVED);
            return;
        }

        // Moverse hacia el destino
        leader.MoveToDestination();

        // Comprobar si se ha llegado
        float dist = Vector3.Distance(leader.transform.position, leader.Destination);
        if (dist <= leader.ArrivalDistance)
        {
            leader.ClearDestination();
            leader.SendInput(leader.INPUT_ARRIVED);
        }
    }

    protected override void OnExit()
    {

    }
}