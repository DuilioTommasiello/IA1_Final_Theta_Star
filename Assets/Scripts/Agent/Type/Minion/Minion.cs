using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : Agent
{
    private FSM fsm;

    [Header("Minion FSM")]
    // Inputs específicos si los hay (por ahora usamos los de Agent)

    private Agent currentTargetEnemy; // se hereda de Agent, pero lo hacemos explícito

    // Propiedades para acceso desde los estados
    public Agent CurrentTargetEnemy => currentTargetEnemy;
    public float AttackRange => attackRange;
    public void Attack() => base.Attack(currentTargetEnemy);

    protected override void Start()
    {
        base.Start();
        InitializeFSM();
    }

    private void InitializeFSM()
    {
        Minion_IdleState idle = new Minion_IdleState(this);
        Minion_AttackState attack = new Minion_AttackState(this);

        idle.AddTransition(INPUT_ENEMY_SPOTTED, attack);
        attack.AddTransition(INPUT_ENEMY_LOST, idle);

        fsm = new FSM(idle);
    }

    public void SendInput(string input)
    {
        Debug.Log($"Minion SendInput: {input} desde estado {fsm.currentState?.GetType().Name}");
        fsm?.SendInput(input);
    }

    private void Update()
    {
        // Detección de enemigos mediante FOV
        Agent visibleEnemy = GetVisibleEnemy();
        if (visibleEnemy != null)
        {
            if (currentTargetEnemy != visibleEnemy)
            {
                currentTargetEnemy = visibleEnemy;
                SendInput(INPUT_ENEMY_SPOTTED);
            }
        }
        else
        {
            if (currentTargetEnemy != null)
            {
                currentTargetEnemy = null;
                SendInput(INPUT_ENEMY_LOST);
            }
        }

        fsm?.Update(Time.deltaTime);

        // Evitación de obstáculos (siempre activa)
        Vector3 rawAvoidance = ObstacleAvoidance(_obstacleMask);
        smoothedAvoidanceForce = Vector3.Lerp(smoothedAvoidanceForce, rawAvoidance, Time.deltaTime / avoidanceSmoothTime);
        if (smoothedAvoidanceForce.magnitude > 0.01f)
            AddForce(smoothedAvoidanceForce);

        ApplyMovement();
    }

    // Para el StateVisualizer
    public override string GetCurrentStateName()
    {
        if (fsm == null || fsm.currentState == null) return "Unknown";
        string fullName = fsm.currentState.GetType().Name;
        return fullName.Replace("Minion_", "").Replace("State", "");
    }
}