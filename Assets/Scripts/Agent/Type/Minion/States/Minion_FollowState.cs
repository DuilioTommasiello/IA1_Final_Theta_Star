using UnityEngine;

public class Minion_FollowState : State
{
    private Minion minion;
    private float checkInterval = 0.5f;
    private float timer = 0f;

    // Variables de formación
    private Vector3 formationOffset;
    private bool hasFormation = false;
    private int formationIndex = -1;

    // Para suavizar el movimiento
    private Vector3 currentVelocity;
    private float smoothTime = 0.3f;

    public Minion_FollowState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log($"{minion.name}: Entering Follow State");

        // Asignar formación si no la tiene
        if (!hasFormation && minion.target != null)
        {
            AssignFormation();
        }

        // Inicializar velocidad suavizada
        currentVelocity = Vector3.zero;

        // Verificar condiciones iniciales
        CheckTransitions();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (minion == null) return;

        timer += deltaTime;

        // Seguir al líder
        if (minion.target != null && minion.target.gameObject.activeSelf)
        {
            FollowLeader();
        }
        else
        {
            // Si no hay líder, detenerse
            minion.AddForce(-minion.velocity * 0.5f);
        }

        // Aplicar flocking
        ApplyFlocking();

        // Evitar obstáculos
        AvoidObstacles();

        // Verificar transiciones
        if (timer >= checkInterval)
        {
            CheckTransitions();
            timer = 0f;
        }
    }

    private void FollowLeader()
    {
        if (!hasFormation)
        {
            AssignFormation();
        }

        // Calcular posición objetivo (formación relativa al líder)
        Vector3 targetPosition = CalculateFormationPosition();

        // Calcular distancia al líder
        float distanceToLeader = Vector3.Distance(minion.transform.position, minion.target.transform.position);

        // Si estamos muy lejos, ir directamente al líder
        if (distanceToLeader > minion._viewRadius * 0.7f)
        {
            targetPosition = minion.target.transform.position;
        }

        // Calcular fuerza de llegada
        Vector3 arriveForce = minion.Arrive(targetPosition);

        // Aplicar fuerza con peso del líder
        minion.AddForce(arriveForce * minion.LeaderWeight);

        // Debug visual
        Debug.DrawLine(minion.transform.position, targetPosition, Color.green);
    }

    private Vector3 CalculateFormationPosition()
    {
        if (minion.target == null) return minion.transform.position;

        Vector3 leaderPos = minion.target.transform.position;
        Vector3 leaderForward = minion.target.transform.forward;
        Vector3 leaderRight = minion.target.transform.right;

        // Si el líder no se está moviendo, usar forward por defecto
        if (leaderForward.magnitude < 0.1f)
        {
            leaderForward = Vector3.forward;
            leaderRight = Vector3.right;
        }

        // Calcular posición en formación circular
        float radius = 4f;
        float angle = formationIndex * (360f / 8) * Mathf.Deg2Rad; // Máximo 8 posiciones

        Vector3 offset = (Mathf.Cos(angle) * radius * leaderRight) +
                        (Mathf.Sin(angle) * radius * leaderForward);

        return leaderPos + offset;
    }

    private void AssignFormation()
    {
        if (minion.target == null) return;

        // Contar minions que siguen al mismo líder
        int count = 0;
        foreach (Minion m in Minion.allMinions)
        {
            if (m != null && m.target == minion.target && m != minion)
                count++;
        }

        // Asignar índice basado en el orden de creación (simplificado)
        formationIndex = count % 8; // Máximo 8 posiciones en formación
        hasFormation = true;

        Debug.Log($"{minion.name}: Formation index {formationIndex} (total followers: {count})");
    }

    private void ApplyFlocking()
    {
        Vector3 flockingForce = minion.CalculateFlockingForce();
        minion.AddForce(flockingForce);
    }

    private void AvoidObstacles()
    {
        if (minion.HasObstacleToAvoid(minion.ObstacleMask))
        {
            Vector3 avoidanceForce = minion.ObstacleAvoidance(minion.ObstacleMask);
            minion.AddForce(avoidanceForce);
        }
    }

    private void CheckTransitions()
    {
        // Verificar si hay enemigos en línea de visión
        if (HasEnemyInSight())
        {
            minion.SendInputToFSM(minion.INPUT_ON_LOS);
            return;
        }

        // Verificar si el líder está fuera de vista
        if (minion.target != null)
        {
            float distance = Vector3.Distance(minion.transform.position, minion.target.transform.position);
            if (distance > minion._viewRadius)
            {
                minion.SendInputToFSM(minion.INPUT_OUT_OF_LOS);
                return;
            }
        }

        // Verificar si la vida es baja
        if (minion.IsLowHealth)
        {
            minion.SendInputToFSM(minion.INPUT_LOWHP);
            return;
        }
    }

    private bool HasEnemyInSight()
    {
        if (minion == null) return false;

        Collider[] hitColliders = Physics.OverlapSphere(minion.transform.position, minion._viewRadius);
        foreach (var hitCollider in hitColliders)
        {
            Agent enemy = hitCollider.GetComponent<Agent>();
            if (enemy != null && enemy != minion && enemy != minion.target)
            {
                // Verificar línea de visión
                Vector3 direction = enemy.transform.position - minion.transform.position;
                float distance = direction.magnitude;

                if (!Physics.Raycast(minion.transform.position, direction.normalized,
                    distance, minion.ObstacleMask))
                {
                    // Verificar si está dentro del ángulo de visión
                    float angle = Vector3.Angle(minion.transform.forward, direction);
                    if (angle < 120f) // Campo de visión de 240 grados
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    protected override void OnExit()
    {
        Debug.Log($"{minion.name}: Exiting Follow State");
    }
}