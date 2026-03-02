using UnityEngine;

public class Leader : Agent
{
    private static Leader selectedLeader = null;

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

    protected override void Start()
    {
        base.Start();

        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        if (selectedLeader == this)
        {
            HandleInput();
            CheckDeselection();
        }

        if (hasDestination)
            MoveToDestination();

        // calculo de fuerza de evitacion suavizada
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

    private void MoveToDestination()
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance <= arrivalDistance)
        {
            hasDestination = false;
            _velocity = Vector3.zero;
            return;
        }

        Vector3 steering = Arrive(destination);
        AddForce(steering);
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
        if (showDebug)
            Debug.Log($"{name} seleccionado");

        if (rend != null)
            rend.material.color = selectedColor;
    }

    private void OnDeselected()
    {
        if (showDebug)
            Debug.Log($"{name} deseleccionado");

        if (rend != null)
            rend.material.color = originalColor;
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