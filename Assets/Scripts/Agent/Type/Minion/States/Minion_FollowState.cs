using UnityEngine;

public class Minion_FollowState : State
{
    private Minion minion;

    public Minion_FollowState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {

    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion.Leader == null) return;

        float distToLeader = Vector3.Distance(minion.transform.position, minion.Leader.transform.position);
        if (distToLeader <= minion.FollowStopDistance)
        {
            minion.SendInput(Minion.INPUT_CLOSE_ENOUGH);
            return; // no aplicar fuerzas si vamos a cambiar de estado
        }

        // Calcular fuerza de flocking
        Vector3 flockingForce = minion.CalculateFlockingForce();

        // Calcular fuerza de arrive hacia el líder
        Vector3 arriveForce = minion.Arrive(minion.Leader.transform.position);

        // Combinar fuerzas
        Vector3 totalForce = flockingForce + arriveForce;
        minion.AddForce(totalForce);
    }

    protected override void OnExit()
    {

    }
}