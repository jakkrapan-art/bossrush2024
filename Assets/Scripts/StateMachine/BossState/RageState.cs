using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RageState : BossStateBase
{
    public RageState(BossStateMachine stateMachine) : base(stateMachine)
    {
    }
    public override void OnEnter()
    {
        Debug.Log("Enter RageState state.");
        base.OnEnter();
    }

    public override void OnExit()
    {
        Debug.Log("Exit RageState state.");
        base.OnExit();
    }
}
