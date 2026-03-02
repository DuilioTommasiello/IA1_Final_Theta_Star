using UnityEngine;

public class Leader : Agent
{
    private static Leader selectedLeader = null;  // Líder actualmente seleccionado

    [Header("RTS Controls")]
    [SerializeField] private LayerMask groundLayer;      // Capa del suelo para el raycast
    [SerializeField] private float arrivalDistance = 1f; // Distancia para considerar destino alcanzado
    [SerializeField] private bool showDebug = true;      // Mostrar gizmos y logs

    [Header("Selection Visual")]
    [SerializeField] private Color selectedColor = Color.yellow;
    private Color originalColor;
    private Renderer rend;

    private Vector3 destination;
    private bool hasDestination = false;

    protected override void Start()
    {
        base.Start();

        // Obtener el renderer para feedback visual
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        // Si no se asignó capa de suelo, intentar buscar una por defecto
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        // Solo el líder seleccionado procesa entrada y movimiento
        if (selectedLeader == this)
        {
            HandleInput();
            CheckDeselection();
        }

        // Movimiento hacia el destino si existe
        if (hasDestination)
            MoveToDestination();

        // --- NUEVA LÓGICA DE EVITACIÓN SUAVIZADA ---
        // Calcular fuerza bruta de evitación (usando el método mejorado de Agent)
        Vector3 rawAvoidance = ObstacleAvoidance(_obstacleMask);
        // Suavizar usando Lerp (evita cambios bruscos)
        smoothedAvoidanceForce = Vector3.Lerp(smoothedAvoidanceForce, rawAvoidance, Time.deltaTime / avoidanceSmoothTime);
        // Aplicar la fuerza suavizada al agente
        if (smoothedAvoidanceForce.magnitude > 0.01f)
            AddForce(smoothedAvoidanceForce);
        // -----------------------------------------

        // Aplicar el movimiento calculado (usando el método de Agent)
        ApplyMovement();
    }

    /// <summary>
    /// Maneja la entrada del usuario para mover al líder seleccionado.
    /// </summary>
    private void HandleInput()
    {
        // Clic derecho: fijar destino
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                destination = hit.point;
                hasDestination = true;
                if (showDebug)
                    Debug.Log($"Destino fijado en {destination}");
            }
        }
    }

    /// <summary>
    /// Comprueba si se debe deseleccionar al líder (clic izquierdo en espacio vacío).
    /// </summary>
    private void CheckDeselection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Si no se clickeó sobre otro líder, deseleccionar
                if (hit.collider.GetComponent<Leader>() == null)
                {
                    Deselect();
                }
            }
            else
            {
                // Clic en el vacío
                Deselect();
            }
        }
    }

    /// <summary>
    /// Movimiento hacia el destino usando Arrive.
    /// </summary>
    private void MoveToDestination()
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance <= arrivalDistance)
        {
            // Destino alcanzado
            hasDestination = false;
            _velocity = Vector3.zero;
            return;
        }

        // Calcular steering con Arrive (frena al acercarse)
        Vector3 steering = Arrive(destination);
        AddForce(steering);
    }

    /// <summary>
    /// Selecciona este líder al hacer clic izquierdo sobre él.
    /// </summary>
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0)) // Solo clic izquierdo
        {
            // Deseleccionar el anterior si existe
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

        // Feedback visual: cambiar color
        if (rend != null)
            rend.material.color = selectedColor;
    }

    private void OnDeselected()
    {
        if (showDebug)
            Debug.Log($"{name} deseleccionado");

        // Restaurar color original
        if (rend != null)
            rend.material.color = originalColor;
    }

    /// <summary>
    /// Método auxiliar para deseleccionar (usado desde CheckDeselection).
    /// </summary>
    private void Deselect()
    {
        if (selectedLeader == this)
        {
            selectedLeader = null;
            OnDeselected();
        }
    }

    // Dibujar gizmos para depuración
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