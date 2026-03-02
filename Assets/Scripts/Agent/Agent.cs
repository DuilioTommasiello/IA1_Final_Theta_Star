using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private Agent _target = default;
    [SerializeField] public float _maxForce = 20, _viewRadius = 20, _stopDistance = 2.0f, _modelRadius = 1.5f; // Aumenté stopDistance
    [SerializeField] public float _maxSpeed = 10;
    [SerializeField] protected Vector3 _velocity = default;
    [SerializeField] private List<Transform> _waypoints = new();
    [SerializeField] protected LayerMask _obstacleMask;
    [SerializeField] protected Team _team = Team.Team1;

    [Header("Obstacle Avoidance")]
    [SerializeField] protected float avoidanceSmoothTime = 0.2f;
    [SerializeField] protected float maxAvoidanceForce = 5f;
    protected Vector3 smoothedAvoidanceForce = Vector3.zero;

    public Agent target { get { return _target; } }
    public Vector3 velocity { get { return _velocity; } }
    public List<Transform> waypoints { get { return _waypoints; } }
    public LayerMask ObstacleMask { get { return _obstacleMask; } }
    public Team team { get { return _team; } }

    public enum Team{
        Team1,
        Team2,
        Team3,
        Team4
    }

    protected virtual void Initialize() { }

    protected virtual void Start() { }

    protected virtual void AgentUpdate()
    {
        ApplyMovement();
    }

    protected void ApplyMovement()
    {
        if (_velocity.magnitude > 0.1f)
        {
            // Aplicar rotación solo si hay movimiento significativo
            Vector3 flatVelocity = new Vector3(_velocity.x, 0, _velocity.z);
            if (flatVelocity.magnitude > 0.1f)
            {
                transform.forward = Vector3.Lerp(transform.forward, flatVelocity.normalized, Time.deltaTime * 5f);
            }

            // Aplicar movimiento
            transform.position += _velocity * Time.deltaTime;
        }
    }

    public void Move()
    {
        if (_velocity == Vector3.zero) return;
        transform.forward = _velocity;
        transform.position += _velocity * Time.deltaTime;
    }

    protected virtual void OnUpdate()
    {
        Movement();
    }

    public void Movement()
    {
        //locomotion
        _velocity.y = 0;
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }

    protected Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        return Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);
    }

    public Vector3 Arrive(Vector3 targetPos)
    {
        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist < _stopDistance) dist = 0;
        if (dist > _viewRadius) return Seek(targetPos);
        return Seek(targetPos, _maxSpeed * (dist / _viewRadius));
    }

    public Vector3 Seek(Vector3 targetPos)
    {
        Vector3 desired = targetPos - transform.position;

        // Si estamos muy cerca, detener
        if (desired.magnitude < _stopDistance)
        {
            return -_velocity * 0.5f; // Fuerza de frenado
        }

        desired.Normalize();
        desired *= _maxSpeed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }

    private Vector3 Seek(Vector3 targetPos, float speed)
    {
        if (targetPos == null || speed <= 0)
        {
            _velocity = Vector3.zero;
            return _velocity;
        }
        //Que vaya al objetivo directamente
        //                  Vo              Vf
        Vector3 desired = (targetPos - transform.position).normalized * speed;

        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);

        return steering;
    }

    public Vector3 AddForce(Vector3 force)
    {
        _velocity += force;

        if (_velocity.magnitude > _maxSpeed)
        {
            _velocity = _velocity.normalized * _maxSpeed;
        }

        _velocity.y = 0;

        return _velocity;
    }

    public Vector3 Flee(Vector3 targetPos)
    {
        return -Seek(targetPos);
    }

    public void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);
        }
    }

    public Vector3 ObstacleAvoidance(LayerMask obstacleMask)
    {
        Vector3 rawAvoidance = Vector3.zero;
        Transform t = transform;

        // Longitudes de rayos más cortas para detectar solo obstáculos cercanos
        float frontRayLength = 2.5f;
        float sideRayLength = 1.8f;

        // Direcciones: frente, diagonales, laterales
        Vector3[] rayDirections = {
        t.forward,
        (t.forward + t.right).normalized,
        (t.forward - t.right).normalized,
        t.right,
        -t.right
    };

        float[] rayLengths = {
        frontRayLength,
        frontRayLength * 0.8f,
        frontRayLength * 0.8f,
        sideRayLength,
        sideRayLength
    };

        float[] rayWeights = { 1.2f, 1.0f, 1.0f, 0.6f, 0.6f }; // Menos peso lateral

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 rayOrigin = t.position + rayDirections[i] * _modelRadius;
            if (Physics.Raycast(rayOrigin, rayDirections[i], out RaycastHit hit, rayLengths[i], obstacleMask))
            {
                float distance = hit.distance;
                // Factor de repulsión cuadrático: más suave al inicio, fuerte al final
                float tFactor = Mathf.Clamp01(distance / rayLengths[i]);
                float repulsionFactor = (1f - tFactor) * (1f - tFactor); // cuadrático

                // Dirección perpendicular al rayo (producto cruz con up)
                Vector3 repelDir = Vector3.Cross(rayDirections[i], Vector3.up).normalized;
                // Elegimos la dirección que aleje del obstáculo: usamos el signo del producto punto con la dirección del obstáculo
                // Pero para simplificar, usamos siempre la misma dirección; en la práctica el agente girará
                rawAvoidance += repelDir * maxAvoidanceForce * rayWeights[i] * repulsionFactor;

                Debug.DrawRay(rayOrigin, rayDirections[i] * rayLengths[i], Color.red);
            }
            else
            {
                Debug.DrawRay(rayOrigin, rayDirections[i] * rayLengths[i], Color.green);
            }
        }

        // Limitar la fuerza bruta para evitar picos
        rawAvoidance = Vector3.ClampMagnitude(rawAvoidance, maxAvoidanceForce);
        return rawAvoidance;
    }



    public bool HasObstacleToAvoid(LayerMask obstacleMaks)
    {
        //Vector3 avoidanceObs = ObstacleAvoidance(obstacleMaks);
        //AddForce(avoidanceObs);
        return ObstacleAvoidance(obstacleMaks) != Vector3.zero;
    }

    public Vector3 Pursuit(Agent target)
    {
        Vector3 futurePos = target.transform.position + target.velocity;

        return Seek(futurePos);
    }

    public Vector3 Evade(Agent target)
    {
        return -Pursuit(target);
    }

    public Vector3 Stop()
    {
        return _velocity = Vector3.zero;
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _viewRadius * 0.5f);

        // direccion
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}