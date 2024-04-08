using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleLastState : ExampleStateBase
{
  public ExampleLastState(ExampleStateMachine stateMachine) : base(stateMachine) { }

  public override void OnEnter()
  {
    Debug.Log("Enter Last state.");
    base.OnEnter();
  }
}
