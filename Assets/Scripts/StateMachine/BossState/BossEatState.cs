using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEatState : BossStateBase
{
  private float _duration = 2f;
  public BossEatState(BossStateMachine stateMachine, Boss boss) : base(stateMachine, boss, "eat")
  {
  }

  public override void OnEnter()
  {
    base.OnEnter();
    Debug.Log("enter eat state.");
  }

  public override void OnExit()
  {
    base.OnExit();
    Debug.Log("exit eat state.");
  }

  public override void Update()
  {
    base.Update();

    if(Time.time >= _startTime + _duration)
    {
      _stateMachine.ChangeState(_stateMachine.PreviousState);
    }
  }
}
