using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0f, 360f)] public float angle;
    [SerializeField, Range(0.1f, 1f)] private float fovRefreshRate = 1f;
    public GameObject agentRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeeAgent;

    private void Start()
    {
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(fovRefreshRate);

        while (true)
        {
            yield return wait;
            FielOfViewCheck();
        }
    }

    private void FielOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if(rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    canSeeAgent = true;
                else
                    canSeeAgent = false;
            }
            else
                canSeeAgent = false;
        }
        else if (canSeeAgent)
            canSeeAgent = false;
    }
}
