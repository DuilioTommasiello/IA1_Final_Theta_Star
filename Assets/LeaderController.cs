using UnityEngine;

public class LeaderController : Agent
{
    [Header("Player Control")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Mouse Look")]
    [SerializeField] private bool useMouseLook = true;
    [SerializeField] private float mouseSensitivity = 2f;

    private Vector3 inputDirection;
    private float currentSpeed;
    private Camera mainCamera;

    protected override void Start()
    {
        base.Start();
        mainCamera = Camera.main;
        currentSpeed = moveSpeed;
    }

    protected override void AgentUpdate()
    {
        HandleInput();
        ApplyMovement();
    }

    private void HandleInput()
    {
        // Movimiento WASD
        inputDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) inputDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDirection -= Vector3.forward;
        if (Input.GetKey(KeyCode.A)) inputDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDirection -= Vector3.left;

        // Normalizar dirección
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // Sprint
        currentSpeed = Input.GetKey(sprintKey) ? moveSpeed * sprintMultiplier : moveSpeed;

        // Rotación con mouse
        if (useMouseLook)
        {
            HandleMouseLook();
        }
        else
        {
            // Rotación con teclas Q/E
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(0, -rotationSpeed, 0);
            if (Input.GetKey(KeyCode.E))
                transform.Rotate(0, rotationSpeed, 0);
        }
    }

    private void HandleMouseLook()
    {
        if (Input.GetMouseButton(1)) // Botón derecho mantiene para rotar
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, mouseX, 0);
        }
    }

    private void ApplyMovement()
    {
        if (inputDirection.magnitude > 0.1f)
        {
            // Convertir dirección de entrada a dirección del mundo
            Vector3 worldDirection = transform.TransformDirection(inputDirection);
            worldDirection.y = 0;
            worldDirection.Normalize();

            // Calcular velocidad deseada
            Vector3 desiredVelocity = worldDirection * currentSpeed;

            // Calcular fuerza de steering
            Vector3 steering = desiredVelocity - _velocity;
            steering = Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);

            // Aplicar fuerza
            AddForce(steering);

            // Rotar en dirección del movimiento si no estamos usando mouse look
            if (!useMouseLook || !Input.GetMouseButton(1))
            {
                transform.forward = Vector3.Lerp(transform.forward, worldDirection, Time.deltaTime * 5f);
            }
        }
        else
        {
            // Frenar suavemente
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, Time.deltaTime * 5f);
        }

        // Aplicar movimiento base
        base.ApplyMovement();
    }

    private void OnGUI()
    {
        // Mostrar controles en pantalla
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = Color.white;
        style.fontSize = 12;

        GUI.Label(new Rect(10, 10, 300, 100),
            "Controles:\n" +
            "WASD - Movimiento\n" +
            "Q/E - Rotación (si Mouse Look desactivado)\n" +
            "Click Derecho + Mouse - Rotar cámara\n" +
            "Shift - Sprint\n" +
            "\nMinions: " + CountFollowers(),
            style);
    }

    private int CountFollowers()
    {
        int count = 0;
        foreach (Minion minion in Minion.allMinions)
        {
            if (minion != null && minion.target == this)
                count++;
        }
        return count;
    }
}