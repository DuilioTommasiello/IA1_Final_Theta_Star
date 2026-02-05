using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : Agent
{
	#region FSM
	public FSM fsm;
	Minion_FollowState followState;
	Minion_AttackState attackState;
	Minion_FleeState fleeState;
	Minion_ChaseState chaseState;
	Minion_HealState healState;
	#endregion

	#region FSM Inputs
	[HideInInspector] public string INPUT_ON_LOS = "OnLOS";
	[HideInInspector] public string INPUT_OUT_OF_LOS = "OutOfLOS";
	[HideInInspector] public string INPUT_LOWHP = "LowHP";
    [HideInInspector] public string INPUT_ENOUGHHP = "EnoughHP";
    [HideInInspector] public string INPUT_LEADERISDEAD = "LeaderIsDead";
    #endregion

    private void Start()
    {
		#region State Init
		followState = new Minion_FollowState(this);
		attackState = new Minion_AttackState(this);
		fleeState = new Minion_FleeState(this);
		chaseState = new Minion_ChaseState(this);
		healState = new Minion_HealState(this);
        #endregion

        #region Transitions

        #region Follow
        followState.AddTransition(INPUT_ON_LOS, attackState);
        followState.AddTransition(INPUT_OUT_OF_LOS, chaseState);
        #endregion

        #region Attack
        attackState.AddTransition(INPUT_LOWHP, fleeState);
        attackState.AddTransition(INPUT_OUT_OF_LOS, chaseState);
        #endregion

        #region Flee
        fleeState.AddTransition(INPUT_ON_LOS, fleeState);
        fleeState.AddTransition(INPUT_OUT_OF_LOS, healState);
        fleeState.AddTransition(INPUT_LEADERISDEAD, chaseState);
        #endregion

        #region Chase
        chaseState.AddTransition(INPUT_ON_LOS, attackState);
        chaseState.AddTransition(INPUT_LOWHP, fleeState);
        #endregion

        #region Heal
        healState.AddTransition(INPUT_ENOUGHHP, followState);
        #endregion

        #endregion
    }
}
