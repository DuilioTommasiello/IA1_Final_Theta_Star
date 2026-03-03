using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float edgeScrollSpeed = 10f;
    [SerializeField] private float edgeScrollThreshold = 10f; // píxeles desde el borde
    [SerializeField] private bool edgeScrolling = true;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 50f;
    [SerializeField] private float zoomSmoothTime = 0.2f;
    private float targetZoom;
    private float zoomVelocity;

    [Header("Boundaries")]
    [SerializeField] private bool enableBoundaries = false;
    [SerializeField] private Vector2 minPosition = new Vector2(-50, -50);
    [SerializeField] private Vector2 maxPosition = new Vector2(50, 50);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        targetZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    void HandlePan()
    {
        Vector3 moveDirection = Vector3.zero;

        // Teclado WASD
        if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

        // Movimiento por bordes de pantalla
        if (edgeScrolling)
        {
            Vector2 mousePos = Input.mousePosition;
            if (mousePos.x >= Screen.width - edgeScrollThreshold) moveDirection += Vector3.right;
            if (mousePos.x <= edgeScrollThreshold) moveDirection += Vector3.left;
            if (mousePos.y >= Screen.height - edgeScrollThreshold) moveDirection += Vector3.forward;
            if (mousePos.y <= edgeScrollThreshold) moveDirection += Vector3.back;
        }

        // Aplicar movimiento
        if (moveDirection != Vector3.zero)
        {
            // Normalizar para que la diagonal no sea más rápida
            moveDirection.Normalize();

            // Convertir a movimiento en el plano horizontal (XZ)
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

            // Mantener la altura actual (y)
            Vector3 newPos = transform.position + movement;

            // Aplicar límites si están habilitados
            if (enableBoundaries)
            {
                newPos.x = Mathf.Clamp(newPos.x, minPosition.x, maxPosition.x);
                newPos.z = Mathf.Clamp(newPos.z, minPosition.y, maxPosition.y); // minPosition.y es para Z
            }

            transform.position = newPos;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }
        else
        {
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }
    }
}