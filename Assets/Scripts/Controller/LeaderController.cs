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
        inputDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) inputDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDirection -= Vector3.forward;
        if (Input.GetKey(KeyCode.A)) inputDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDirection -= Vector3.left;

        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize();

        currentSpeed = Input.GetKey(sprintKey) ? moveSpeed * sprintMultiplier : moveSpeed;

        if (useMouseLook)
            HandleMouseLook();
        else
        {
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(0, -rotationSpeed, 0);
            if (Input.GetKey(KeyCode.E))
                transform.Rotate(0, rotationSpeed, 0);
        }
    }

    private void HandleMouseLook()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, mouseX, 0);
        }
    }

    private void ApplyMovement()
    {
        if (inputDirection.magnitude > 0.1f)
        {
            Vector3 worldDirection = transform.TransformDirection(inputDirection);
            worldDirection.y = 0;
            worldDirection.Normalize();

            Vector3 desiredVelocity = worldDirection * currentSpeed;
            Vector3 steering = desiredVelocity - _velocity;
            steering = Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);

            AddForce(steering);

            if (!useMouseLook || !Input.GetMouseButton(1))
                transform.forward = Vector3.Lerp(transform.forward, worldDirection, Time.deltaTime * 5f);
        }
        else
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, Time.deltaTime * 5f);

        base.ApplyMovement();
    }
}