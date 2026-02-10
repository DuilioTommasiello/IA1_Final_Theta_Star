using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : Agent
{
    #region Variables

    #region Stats
    [Header("Minion Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth = 100f;
    [SerializeField] private float _healRate = 10f;
    [SerializeField] private float _lowHealthThreshold = 0.2f; // 20%
    [SerializeField] private float _attackRange = 10f;
    [SerializeField] private float _fleeDistance = 30f;


    #endregion

    #region Attack
    [Header("Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float lastAttackTime = 0f;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Agent currentTargetEnemy = null;
    #endregion

    #region Flee
    [Header("Flee Settings")]
    [SerializeField] private float safeDistance = 25f;
    [SerializeField] private float fleeSpeedMultiplier = 1.5f;
    private Vector3 safePosition = Vector3.zero;
    private bool isHealing = false;
    #endregion

    #region Chase
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.2f;
    #endregion

    #region Getters
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float HealthPercentage => _currentHealth / _maxHealth;
    public bool IsLowHealth => HealthPercentage <= _lowHealthThreshold;
    public float AttackRange => _attackRange;
    public float FleeDistance => _fleeDistance;
    public float HealRate => _healRate;
    public Agent CurrentTargetEnemy { get => currentTargetEnemy; set => currentTargetEnemy = value; }
    public float SafeDistance => safeDistance;
    public float FleeSpeedMultiplier => fleeSpeedMultiplier;
    public float ChaseSpeedMultiplier => chaseSpeedMultiplier;

    public bool CanAttack => Time.time - lastAttackTime >= attackCooldown;
    #endregion

    #region Flocking
    private FlockManager flockManager;
    [Header("Flocking Settings")]
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.2f;
    [SerializeField] private float leaderWeight = 2.0f;
    [SerializeField] private float separationRadius = 2.0f;
    [SerializeField] private float neighborRadius = 5.0f;
    [SerializeField] private float maxFlockingForce = 5.0f;

    private Vector3 formationOffset = Vector3.zero;
    private List<Minion> flockMates = new List<Minion>();
    public static List<Minion> allMinions = new List<Minion>();

    public float SeparationWeight => separationWeight;
    public float AlignmentWeight => alignmentWeight;
    public float CohesionWeight => cohesionWeight;
    public float LeaderWeight => leaderWeight;
    public float SeparationRadius => separationRadius;
    public float NeighborRadius => neighborRadius;
    public float MaxFlockingForce => maxFlockingForce;

    public Vector3 FormationOffset
    {
        get => formationOffset;
        set => formationOffset = value;
    }
    #endregion

    #region FSM
    public FSM fsm;
	Minion_FollowState followState;
	Minion_AttackState attackState;
	Minion_FleeState fleeState;
	Minion_ChaseState chaseState;
	Minion_HealState healState;
	#endregion

	#region FSM Inputs
	[HideInInspector] public string INPUT_ON_LOS = "OnLOS";
	[HideInInspector] public string INPUT_OUT_OF_LOS = "OutOfLOS";
	[HideInInspector] public string INPUT_LOWHP = "LowHP";
    [HideInInspector] public string INPUT_ENOUGHHP = "EnoughHP";
    [HideInInspector] public string INPUT_LEADERISDEAD = "LeaderIsDead";
    #endregion

    #endregion

    protected override void Start()
    {
        base.Start();

        #region Flocking
        flockManager = FindObjectOfType<FlockManager>();
        if (flockManager == null)
        {
            GameObject go = new GameObject("FlockManager");
            flockManager = go.AddComponent<FlockManager>();
        }
        else
        {
            StartCoroutine(RegisterWithFlockManager());
        }

        // Registrar en el FlockManager
        if (flockManager != null && target != null)
        {
            flockManager.RegisterMinion(this, target);
        }
        #endregion

        #region State Init
        followState = new Minion_FollowState(this);
		attackState = new Minion_AttackState(this);
		fleeState = new Minion_FleeState(this);
		chaseState = new Minion_ChaseState(this);
		healState = new Minion_HealState(this);
        #endregion

        #region Transitions

        #region Follow
        followState.AddTransition(INPUT_ON_LOS, attackState);
        followState.AddTransition(INPUT_OUT_OF_LOS, chaseState);
        followState.AddTransition(INPUT_LOWHP, fleeState);
        #endregion

        #region Attack
        attackState.AddTransition(INPUT_LOWHP, fleeState);
        attackState.AddTransition(INPUT_OUT_OF_LOS, chaseState);
        #endregion

        #region Flee
        fleeState.AddTransition(INPUT_ON_LOS, fleeState);
        fleeState.AddTransition(INPUT_OUT_OF_LOS, healState);
        fleeState.AddTransition(INPUT_LEADERISDEAD, chaseState);
        #endregion

        #region Chase
        chaseState.AddTransition(INPUT_ON_LOS, attackState);
        chaseState.AddTransition(INPUT_LOWHP, fleeState);
        #endregion

        #region Heal
        healState.AddTransition(INPUT_ENOUGHHP, followState);
        healState.AddTransition(INPUT_ON_LOS, fleeState);
        #endregion

        #endregion

        fsm = new FSM(followState);
    }

    private void Update()
    {
        fsm?.Update(Time.deltaTime);
        AgentUpdate();
    }

    #region Flee
    public void SetSafePosition(Vector3 position)
    {
        safePosition = position;
    }

    public Vector3 GetSafePosition()
    {
        return safePosition;
    }
    #endregion

    public Agent FindClosestEnemy()
    {
        Agent closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _viewRadius);
        foreach (var collider in hitColliders)
        {
            Agent agent = collider.GetComponent<Agent>();
            if (agent != null && agent != this && agent != target)
            {
                // Verificar si es un enemigo (esto dependerá de tu sistema de equipos)
                float distance = Vector3.Distance(transform.position, agent.transform.position);
                if (distance < closestDistance)
                {
                    // Verificar línea de visión
                    if (!Physics.Raycast(transform.position,
                        (agent.transform.position - transform.position).normalized,
                        distance, _obstacleMask))
                    {
                        closestEnemy = agent;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestEnemy;
    }

    public Agent FindClosestEnemyExcludingFlee()
    {
        Agent closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _viewRadius);
        foreach (var collider in hitColliders)
        {
            Agent agent = collider.GetComponent<Agent>();
            if (agent != null && agent != this && agent != target)
            {
                // Aquí necesitarías verificar si el enemigo está en estado Flee
                // Esto requeriría acceso al FSM del otro agente o un sistema de estados visibles
                // Por ahora, asumiremos que todos los enemigos son válidos
                float distance = Vector3.Distance(transform.position, agent.transform.position);
                if (distance < closestDistance)
                {
                    if (!Physics.Raycast(transform.position,
                        (agent.transform.position - transform.position).normalized,
                        distance, _obstacleMask))
                    {
                        closestEnemy = agent;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestEnemy;
    }

    public void Attack(Agent targetEnemy)
    {
        if (!CanAttack || targetEnemy == null) return;

        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = GameObject.Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.identity
            );

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(targetEnemy.transform, attackDamage);
            }

            lastAttackTime = Time.time;
            Debug.Log($"{name}: Attacking {targetEnemy.name}");
        }
    }

    public Vector3 CalculateSafeFleePosition()
    {
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = 0;
        randomDirection.Normalize();

        return transform.position + randomDirection * safeDistance;
    }

    public bool IsAtSafePosition()
    {
        if (safePosition == Vector3.zero) return false;

        return Vector3.Distance(transform.position, safePosition) < _stopDistance;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);

        Debug.Log($"{name} took {damage} damage. Health: {_currentHealth}/{_maxHealth}");

        if (IsLowHealth && fsm != null)
        {
            SendInputToFSM(INPUT_LOWHP);
        }
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

        if (!IsLowHealth && fsm != null)
        {
            SendInputToFSM(INPUT_ENOUGHHP);
        }
    }

    public List<Minion> GetNearbyFlockMates()
    {
        if (flockManager != null)
        {
            return flockManager.GetNearbyMates(this, neighborRadius);
        }

        // Fallback al método original
        List<Minion> nearbyMates = new List<Minion>();

        foreach (Minion mate in allMinions)
        {
            if (mate == this || mate == null || mate.target != target) continue;

            float distance = Vector3.Distance(transform.position, mate.transform.position);
            if (distance <= neighborRadius)
            {
                nearbyMates.Add(mate);
            }
        }

        return nearbyMates;
    }

    public Vector3 CalculateSeparation(List<Minion> flockMates)
    {
        Vector3 separationForce = Vector3.zero;
        int count = 0;

        foreach (Minion mate in flockMates)
        {
            float distance = Vector3.Distance(transform.position, mate.transform.position);

            if (distance > 0 && distance < separationRadius)
            {
                Vector3 diff = transform.position - mate.transform.position;
                diff.Normalize();
                diff /= distance; // La fuerza es inversamente proporcional a la distancia
                separationForce += diff;
                count++;
            }
        }

        if (count > 0)
        {
            separationForce /= count;
        }

        return separationForce;
    }

    // Fuerza de Alineación: Alinear velocidad con compañeros cercanos
    public Vector3 CalculateAlignment(List<Minion> flockMates)
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;

        foreach (Minion mate in flockMates)
        {
            averageVelocity += mate.velocity;
            count++;
        }

        if (count > 0)
        {
            averageVelocity /= count;
            averageVelocity.Normalize();
        }

        return averageVelocity;
    }

    // Fuerza de Cohesión: Moverse hacia el centro de masa del grupo
    public Vector3 CalculateCohesion(List<Minion> flockMates)
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;

        foreach (Minion mate in flockMates)
        {
            centerOfMass += mate.transform.position;
            count++;
        }

        if (count > 0)
        {
            centerOfMass /= count;
            Vector3 cohesionForce = centerOfMass - transform.position;
            cohesionForce.Normalize();
            return cohesionForce;
        }

        return Vector3.zero;
    }

    // Fuerza combinada de flocking
    public Vector3 CalculateFlockingForce()
    {
        List<Minion> nearbyMates = GetNearbyFlockMates();

        if (nearbyMates.Count == 0)
            return Vector3.zero;

        Vector3 separation = CalculateSeparation(nearbyMates) * separationWeight;
        Vector3 alignment = CalculateAlignment(nearbyMates) * alignmentWeight;
        Vector3 cohesion = CalculateCohesion(nearbyMates) * cohesionWeight;

        Vector3 flockingForce = separation + alignment + cohesion;

        if (flockingForce.magnitude > maxFlockingForce)
        {
            flockingForce = flockingForce.normalized * maxFlockingForce;
        }

        return flockingForce;
    }

    private IEnumerator RegisterWithFlockManager()
    {
        yield return new WaitForSeconds(0.1f);

        if (flockManager != null && target != null)
        {
            flockManager.RegisterMinion(this, target);
        }
    }


    private void OnEnable()
    {
        if (!allMinions.Contains(this))
        {
            allMinions.Add(this);
        }
    }

    private void OnDisable()
    {
        if (allMinions.Contains(this))
        {
            allMinions.Remove(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (flockManager != null && target != null)
        {
            flockManager.UnregisterMinion(this, target);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            // vfx
        }
    }

    public void SendInputToFSM(string input)
    {
        fsm?.SendInput(input);
    }
    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        // Dibujar rango de ataque
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Dibujar distancia segura
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        // Dibujar posición segura actual
        if (Application.isPlaying && GetSafePosition() != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetSafePosition(), 1f);
            Gizmos.DrawLine(transform.position, GetSafePosition());
        }

        // Dibujar objetivo actual
        if (CurrentTargetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, CurrentTargetEnemy.transform.position);
        }
    }
    #endregion
}
