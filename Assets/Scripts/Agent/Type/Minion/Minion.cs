using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : Agent
{
    private FSM fsm;

    [Header("FSM Inputs")]
    [HideInInspector] public string INPUT_ENEMY_SPOTTED = "EnemySpotted";
    [HideInInspector] public string INPUT_ENEMY_LOST = "EnemyLost";

    [Header("Leader")]
    [SerializeField] private Leader leader;
    public Leader Leader => leader;

    [Header("Flocking Settings")]
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.2f;
    [SerializeField] private float leaderWeight = 2.0f;
    [SerializeField] private float separationRadius = 2.0f;
    [SerializeField] private float neighborRadius = 5.0f;
    [SerializeField] private float maxFlockingForce = 5.0f;

    public static List<Minion> allMinions = new List<Minion>();

    private Agent currentTargetEnemy;

    public Agent CurrentTargetEnemy => currentTargetEnemy;
    public float AttackRange => attackRange;
    public void Attack() => base.Attack(currentTargetEnemy);

    protected override void Start()
    {
        base.Start();
        
        if(!allMinions.Contains(this))
            allMinions.Add(this);

        if(leader == null)
        {
            Debug.LogWarning($"Asigna un Lider en {this.gameObject.name}");
        }

        InitializeFSM();
    }

    private void InitializeFSM()
    {
        Minion_FollowState follow = new Minion_FollowState(this);
        Minion_AttackState attack = new Minion_AttackState(this);

        follow.AddTransition(INPUT_ENEMY_SPOTTED, attack);
        attack.AddTransition(INPUT_ENEMY_LOST, follow);

        fsm = new FSM(follow);
    }

    public void SendInput(string input)
    {
        Debug.Log($"Minion SendInput: {input} desde estado {fsm.currentState?.GetType().Name}");
        fsm?.SendInput(input);
    }

    private void Update()
    {
        // Detección de enemigos mediante FOV
        Agent visibleEnemy = GetVisibleEnemy();
        if (visibleEnemy != null)
        {
            if (currentTargetEnemy != visibleEnemy)
            {
                currentTargetEnemy = visibleEnemy;
                SendInput(INPUT_ENEMY_SPOTTED);
            }
        }
        else
        {
            if (currentTargetEnemy != null)
            {
                currentTargetEnemy = null;
                SendInput(INPUT_ENEMY_LOST);
            }
        }

        fsm?.Update(Time.deltaTime);

        // Evitación de obstáculos (siempre activa)
        Vector3 rawAvoidance = ObstacleAvoidance(_obstacleMask);
        smoothedAvoidanceForce = Vector3.Lerp(smoothedAvoidanceForce, rawAvoidance, Time.deltaTime / avoidanceSmoothTime);
        if (smoothedAvoidanceForce.magnitude > 0.01f)
            AddForce(smoothedAvoidanceForce);

        ApplyMovement();
    }

    public override string GetCurrentStateName()
    {
        if (fsm == null || fsm.currentState == null) return "Unknown";
        string fullName = fsm.currentState.GetType().Name;
        return fullName.Replace("Minion_", "").Replace("State", "");
    }

    #region Flocking
    private List<Minion> GetNearbyFlockMates()
    {
        List<Minion> nearby = new List<Minion>();
        foreach (Minion other in allMinions)
        {
            if (other == this || other == null || other.leader != this.leader) continue;
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist <= neighborRadius)
                nearby.Add(other);
        }
        return nearby;
    }

    /// <summary>
    /// Fuerza de separación: evita amontonarse.
    /// </summary>
    private Vector3 CalculateSeparation(List<Minion> mates)
    {
        Vector3 force = Vector3.zero;
        int count = 0;
        foreach (Minion mate in mates)
        {
            float dist = Vector3.Distance(transform.position, mate.transform.position);
            if (dist > 0 && dist < separationRadius)
            {
                Vector3 away = (transform.position - mate.transform.position).normalized;
                away /= dist; // cuanto más cerca, más fuerza
                force += away;
                count++;
            }
        }
        if (count > 0)
            force /= count;
        return force;
    }

    /// <summary>
    /// Fuerza de alineación: igualar velocidad con los vecinos.
    /// </summary>
    private Vector3 CalculateAlignment(List<Minion> mates)
    {
        Vector3 avgVelocity = Vector3.zero;
        int count = 0;
        foreach (Minion mate in mates)
        {
            avgVelocity += mate.velocity;
            count++;
        }
        if (count > 0)
        {
            avgVelocity /= count;
            avgVelocity.Normalize();
        }
        return avgVelocity;
    }

    /// <summary>
    /// Fuerza de cohesión: moverse hacia el centro de masa de los vecinos.
    /// </summary>
    private Vector3 CalculateCohesion(List<Minion> mates)
    {
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (Minion mate in mates)
        {
            center += mate.transform.position;
            count++;
        }
        if (count > 0)
        {
            center /= count;
            Vector3 direction = (center - transform.position).normalized;
            return direction;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Calcula la fuerza total de flocking (sin incluir líder).
    /// </summary>
    public Vector3 CalculateFlockingForce()
    {
        List<Minion> mates = GetNearbyFlockMates();
        if (mates.Count == 0) return Vector3.zero;

        Vector3 separation = CalculateSeparation(mates) * separationWeight;
        Vector3 alignment = CalculateAlignment(mates) * alignmentWeight;
        Vector3 cohesion = CalculateCohesion(mates) * cohesionWeight;

        Vector3 flocking = separation + alignment + cohesion;
        if (flocking.magnitude > maxFlockingForce)
            flocking = flocking.normalized * maxFlockingForce;
        return flocking;
    }
    #endregion

    private void OnDestroy()
    {
        if (allMinions.Contains(this))
            allMinions.Remove(this);
    }
}