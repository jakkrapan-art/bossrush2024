using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BossStateBase
{
    public IdleState(BossStateMachine stateMachine, Boss boss) : base(stateMachine, boss, "idle", true)
    {
    }
    public override void OnEnter()
    {
        Debug.Log("Enter IdleState state.");
        base.OnEnter();
    }

    public override void OnExit()
    {
        Debug.Log("Exit IdleState state.");
        base.OnExit();
    }
}