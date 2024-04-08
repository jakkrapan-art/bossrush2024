using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlmostFullState : BossStateBase
{
    public AlmostFullState(BossStateMachine stateMachine) : base(stateMachine)
    {

    }
    public override void OnEnter()
    {
        Debug.Log("Enter AlmostFull state.");
        base.OnEnter();
    }

    public override void OnExit()
    {
        Debug.Log("Exit AlmostFull state.");
        base.OnExit();
    }
}
