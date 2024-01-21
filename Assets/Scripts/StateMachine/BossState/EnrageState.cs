using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnrageState : BossStateBase
{
    public EnrageState(BossStateMachine stateMachine) : base(stateMachine)
    {
    }
    public override void OnEnter()
    {
        Debug.Log("Enter EnrageState state.");
        base.OnEnter();
    }

    public override void OnExit()
    {
        Debug.Log("Exit EnrageState state.");
        base.OnExit();
    }
}
