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
        if (minion.Leader == null) return; // No hay líder, no hacer nada

        // Calcular fuerza de flocking (separación, alineación, cohesión)
        Vector3 flockingForce = minion.CalculateFlockingForce();

        // Calcular fuerza de arrive hacia el líder
        Vector3 arriveForce = minion.Arrive(minion.Leader.transform.position);

        // Combinar fuerzas (puedes ajustar los pesos)
        Vector3 totalForce = flockingForce + arriveForce;

        // Aplicar la fuerza (limitada por maxForce)
        minion.AddForce(totalForce);
    }

    protected override void OnExit()
    {
        // No es necesario
    }
}