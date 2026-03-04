using UnityEngine;

public class Minion_BerserkState : State
{
    private Minion minion;
    private float searchTimer;
    private float searchInterval = 2f;
    private Vector3 searchDirection;

    public Minion_BerserkState(Agent agent) : base(agent)
    {
        minion = agent as Minion;
    }

    protected override void OnEnter()
    {
        Debug.Log("Minion Berserk State Enter");
        searchTimer = 0f;
        PickNewSearchDirection();
    }

    protected override void OnUpdate(float deltaTime)
    {
        Agent enemy = minion.GetVisibleEnemy();
        if (enemy != null)
        {
            minion.LookAt(enemy.transform.position);
            if (minion.CanAttack)
            {
                minion.Attack(enemy);
            }
        }
        else
        {
            searchTimer += deltaTime;
            if (searchTimer >= searchInterval)
            {
                PickNewSearchDirection();
                searchTimer = 0f;
            }

            Vector3 desired = searchDirection * minion._maxSpeed;
            Vector3 steering = desired - minion.velocity;
            steering = Vector3.ClampMagnitude(steering, minion._maxForce);
            minion.AddForce(steering);
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