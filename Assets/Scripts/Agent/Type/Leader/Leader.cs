using UnityEngine;

public class Leader : Agent
{
    private static Leader selectedLeader = null;
    private FSM fsm;

    [Header("RTS Controls")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float arrivalDistance = 1f;

    [SerializeField] private bool showDebug = true;

    [Header("Selection Visual")]
    [SerializeField] private Color selectedColor = Color.yellow;
    private Color originalColor;
    private Renderer rend;

    private Vector3 destination;
    private bool hasDestination = false;

    [Header("FSM Inputs")]
    [HideInInspector] public string INPUT_MOVE_ORDER = "MoveOrder";
    [HideInInspector] public string INPUT_ARRIVED = "Arrived";
    [HideInInspector] public const string INPUT_ENEMY_LOST_TO_IDLE = "LostToIdle"; 
    [HideInInspector] public const string INPUT_ENEMY_LOST_TO_MOVE = "LostToMove";

    private Agent currentTargetEnemy;
    private bool hadDestinationBeforeAttack;

    // getters para acceso desde estados
    public bool HasDestination => hasDestination;
    public Vector3 Destination => destination;
    public float ArrivalDistance => arrivalDistance;

    protected override void Start()
    {
        base.Start();

        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");

        InitializeFSM();
    }

    private void InitializeFSM()
    {
        Leader_IdleState idle = new Leader_IdleState(this);
        Leader_MoveState move = new Leader_MoveState(this);
        Leader_AttackState attack = new Leader_AttackState(this);

        idle.AddTransition(INPUT_MOVE_ORDER, move);
        idle.AddTransition(INPUT_ENEMY_SPOTTED, attack);

        move.AddTransition(INPUT_ARRIVED, idle);
        move.AddTransition(INPUT_ENEMY_SPOTTED, attack);

        attack.AddTransition(INPUT_ENEMY_LOST_TO_IDLE, idle);
        attack.AddTransition(INPUT_ENEMY_LOST_TO_MOVE, move);

        fsm = new FSM(idle);
    }

    public void SendInput(string input) => fsm?.SendInput(input);

    private void Update()
    {
        if (selectedLeader == this)
        {
            HandleInput();
            CheckDeselection();
        }

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

        Vector3 rawAvoidance = ObstacleAvoidance(_obstacleMask);
        smoothedAvoidanceForce = Vector3.Lerp(smoothedAvoidanceForce, rawAvoidance, Time.deltaTime / avoidanceSmoothTime);
        if (smoothedAvoidanceForce.magnitude > 0.01f)
            AddForce(smoothedAvoidanceForce);

        ApplyMovement();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                destination = hit.point;
                hasDestination = true;
                //if (showDebug)
                //    Debug.Log($"destino fijado en {destination}");

                // Enviar orden de movimiento a la FSM
                SendInput(INPUT_MOVE_ORDER);
            }
        }
    }

    private void CheckDeselection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<Leader>() == null)
                    Deselect();
            }
            else
                Deselect();
        }
    }

    public void MoveToDestination()
    {
        Vector3 steering = Arrive(destination);
        AddForce(steering);
    }

    public void ClearDestination()
    {
        hasDestination = false;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedLeader != null && selectedLeader != this)
                selectedLeader.OnDeselected();

            selectedLeader = this;
            OnSelected();
        }
    }

    private void OnSelected()
    {
        if (showDebug) Debug.Log($"{name} seleccionado");
        if (rend != null) rend.material.color = selectedColor;
    }

    private void OnDeselected()
    {
        if (showDebug) Debug.Log($"{name} deseleccionado");
        if (rend != null) rend.material.color = originalColor;
    }

    private void Deselect()
    {
        if (selectedLeader == this)
        {
            selectedLeader = null;
            OnDeselected();
        }
    }

    private void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (hasDestination && showDebug)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(destination, 0.5f);
            Gizmos.DrawLine(transform.position, destination);
        }
    }
}