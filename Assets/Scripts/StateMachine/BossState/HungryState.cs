using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungryState : BossStateBase
{
    public HungryState(BossStateMachine stateMachine, Boss boss) : base(stateMachine, boss) { }
    public override void OnEnter()
    {
        Debug.Log("Enter HungryState state.");
        base.OnEnter();
    }

    public override void OnExit()
    {
        Debug.Log("Exit HungryState state.");
        base.OnExit();
    }
}
