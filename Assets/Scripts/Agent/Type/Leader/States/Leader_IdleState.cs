using UnityEngine;
using KevinIglesias;

public class Leader_IdleState : State
{
    private Leader leader;

    public Leader_IdleState(Agent agent) : base(agent)
    {
        leader = agent as Leader;
    }

    protected override void OnEnter()
    {
        leader.Stop();
        if(leader.soldierController != null)
        {
            leader.soldierController.movement = SoldierMovement.NoMovement;
            leader.soldierController.action = SoldierAction.HoldWeapon;
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        //Debug.Log($"IdleState.Update: HasDestination = {leader.HasDestination}");
        if (leader.HasDestination)
        {
            Debug.Log("Enviando MoveOrder desde IdleState");
            leader.SendInput("MoveOrder");
        }
    }

    protected override void OnExit()
    {
        // No es necesario hacer nada al salir
    }
}