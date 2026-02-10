using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : Agent
{
    [Header("Leader Movement")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolSpeed = 5f;
    [SerializeField] private bool patrolRandomly = true;

    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private float patrolChangeTime = 0f;
    private float patrolChangeInterval = 3f;

    protected override void Start()
    {
        base.Start();
        patrolCenter = transform.position;
        SetNewPatrolTarget();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (patrolRandomly)
        {
            PatrolMovement();
        }
    }

    private void PatrolMovement()
    {
        patrolChangeTime += Time.deltaTime;

        if (patrolChangeTime >= patrolChangeInterval ||
            Vector3.Distance(transform.position, currentPatrolTarget) < _stopDistance)
        {
            SetNewPatrolTarget();
            patrolChangeTime = 0f;
        }

        // Mover hacia el objetivo de patrulla
        Vector3 steering = Arrive(currentPatrolTarget);
        _velocity = AddForce(steering);

        // Rotar en la dirección del movimiento
        if (_velocity.magnitude > 0.1f)
        {
            transform.forward = Vector3.Lerp(transform.forward, _velocity.normalized, Time.deltaTime * 5f);
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

        Debug.DrawLine(transform.position, currentPatrolTarget, Color.cyan, patrolChangeInterval);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(currentPatrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
        }
    }
}