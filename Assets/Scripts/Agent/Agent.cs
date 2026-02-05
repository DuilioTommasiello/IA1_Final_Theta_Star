using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private Agent _target = default;
    [SerializeField] public float _maxForce = 20, _viewRadius = 20, _stopDistance = 0.02f, _modelRadius = 1.5f;
    [SerializeField] public float _maxSpeed = 10;
    [SerializeField] protected Vector3 _velocity = default;
    [SerializeField] private List<Transform> _waypoints = new();
    [SerializeField] private LayerMask _obstacleMask;
    
    public Agent target { get { return _target; } }
    public Vector3 velocity { get { return _velocity; } }
    public List<Transform> waypoints { get { return _waypoints; } }
    public LayerMask ObstacleMask { get { return _obstacleMask; } }

    public enum Team{
        Team1,
        Team2,
        Team3,
        Team4
    } 

    public void Move()
    {
        if (_velocity == Vector3.zero) return;
        transform.forward = _velocity;
        transform.position += _velocity * Time.deltaTime;
    }

    public void OnUpdate()
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
        if (targetPos == null) return default;

        Vector3 desired = targetPos - transform.position;
        desired.Normalize();
        desired *= _maxSpeed;

        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);

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
        return _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);
    }

    public Vector3 Flee(Vector3 targetPos)
    {
        return -Seek(targetPos);
    }

    public Vector3 ObstacleAvoidance(LayerMask obstacleMask)
    {
        Vector3 avoidanceForce = Vector3.zero;
        Transform t = transform; // cacheo el transform

        // Raycast modifiers
        Vector3[] rayDirections = {
        t.forward,                         // Forward
        -t.forward,                        // Backward
        -t.right,                          // Left
        t.right                            // Right
    };

        float[] rayMultipliers = {
        _viewRadius * 0.5f,               // Forward range
        _viewRadius * 0.5f,               // Backward range
        _viewRadius * 0.3f,               // Side range
        _viewRadius * 0.3f                // Side range
    };

        Vector3[] avoidanceOffsets = {
        -t.right,                         // Avoid to the left
        t.right,                          // Avoid to the right
        t.right,                          // Avoid to the right
        -t.right                          // Avoid to the left
    };

        // raycasts
        for (int i = 0; i < rayDirections.Length; i++)
        {
            if (Physics.Raycast(t.position + rayDirections[i] * _modelRadius, rayDirections[i], rayMultipliers[i], obstacleMask))
            {
                avoidanceForce += Seek(t.position + avoidanceOffsets[i]);
                
                Debug.DrawRay(t.position + rayDirections[i] * _modelRadius, rayDirections[i] * rayMultipliers[i], Color.red);
                return avoidanceForce;
            }
        }

        return avoidanceForce;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _viewRadius * 0.5f);
    }
}