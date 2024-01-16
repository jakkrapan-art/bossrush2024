using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleFirstState : ExampleStateBase
{
  private float _duration;

  public ExampleFirstState(ExampleStateMachine stateMachine, float duration) : base(stateMachine)
  {
    _duration = duration;
  }

  public override void OnEnter()
  {
    Debug.Log("Enter First state.");
    base.OnEnter();
  }

  public override void Update()
  {
    if (_startTime + _duration < Time.time) _stateMachine.ChangeState(_stateMachine.SecondState);
  }

  public override void OnExit()
  {
    Debug.Log("Exit First state.");
    base.OnExit();
  }
}
