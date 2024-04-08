using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThinkState : BossStateBase
{
  float think_time = 0;
  public ThinkState(BossStateMachine stateMachine, float think_time = 20) : base(stateMachine)
  {
    this.think_time = think_time;
  }
  public override void OnEnter()
  {
    Debug.Log("Enter ThinkState state.");
    _stateMachine.Boss.ShowThinking(null);
    base.OnEnter();
  }

  public override void Update()
  {
    if (Time.time >= _startTime + think_time)
    {
      //TODO: Show require food.

      _stateMachine.ChangeState(_stateMachine.PreviousState);
    }
    base.Update();
  }

  public override void OnExit()
  {
    Debug.Log("Exit ThinkState state.");
    _stateMachine.Boss.HideThinking();
    base.OnExit();
  }
}
