using KevinIglesias;
using UnityEngine;

public class Minion_BerserkState : State
{
    private Minion minion;
    private float searchTimer;
    private float searchInterval = 2f;
    private Vector3 searchDirection;
    private Vector3 lastKnownEnemyPosition;
    private bool hasLastKnownPosition;

    public Minion_BerserkState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log("Minion Berserk State Enter");
        searchTimer = 0f;
        hasLastKnownPosition = false;
        PickNewSearchDirection();

        if (minion.soldierController != null)
        {
            minion.soldierController.movement = SoldierMovement.Run;
            minion.soldierController.action = SoldierAction.Shoot02;
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        Agent enemy = minion.GetVisibleEnemy();
        if (enemy != null)
        {
            lastKnownEnemyPosition = enemy.transform.position;
            hasLastKnownPosition = true;

            minion.LookAt(enemy.transform.position);
            if (minion.CanAttack)
                minion.Attack(enemy);
        }
        else
        {
            Vector3 targetPosition = Vector3.zero;
            bool hasTarget = false;

            if (hasLastKnownPosition)
            {
                targetPosition = lastKnownEnemyPosition;
                hasTarget = true;

                if (Vector3.Distance(minion.transform.position, lastKnownEnemyPosition) < 1f)
                    hasLastKnownPosition = false;
            }
            else
            {
                Leader enemyLeader = minion.GetClosestEnemyLeader();
                if (enemyLeader != null)
                {
                    targetPosition = enemyLeader.transform.position;
                    hasTarget = true;
                }
                else
                {
                    searchTimer += deltaTime;
                    if (searchTimer >= searchInterval)
                    {
                        PickNewSearchDirection();
                        searchTimer = 0f;
                    }
                    targetPosition = minion.transform.position + searchDirection * 10f;
                    hasTarget = true;
                }
            }

            if (hasTarget)
            {
                Vector3 desired = (targetPosition - minion.transform.position).normalized * minion._maxSpeed;
                Vector3 steering = desired - minion.velocity;
                steering = Vector3.ClampMagnitude(steering, minion._maxForce);
                minion.AddForce(steering);
            }
        }
    }

    private void PickNewSearchDirection()
    {
        float angle = Random.Range(0f, 360f);
        searchDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
    }

    protected override void OnExit()
    {
        Debug.Log("Minion Berserk State Exit");
    }
}