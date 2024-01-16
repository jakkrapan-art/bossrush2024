using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleSecondState : ExampleStateBase
{
  private float _duration;

  public ExampleSecondState(ExampleStateMachine stateMachine, float duration) : base(stateMachine)
  {
    _duration = duration;
  }

  public override void OnEnter()
  {
    Debug.Log("Enter Second state.");
    base.OnEnter();
  }

  public override void Update()
  {
    if (_startTime + _duration < Time.time) _stateMachine.ChangeState(_stateMachine.LastState);
  }

  public override void OnExit()
  {
    Debug.Log("Exit Second state.");
    base.OnExit();
  }
}
