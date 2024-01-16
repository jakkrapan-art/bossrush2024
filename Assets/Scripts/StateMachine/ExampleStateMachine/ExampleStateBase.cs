using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleStateBase : State
{
  protected ExampleStateMachine _stateMachine = null;
  public ExampleStateBase(ExampleStateMachine stateMachine)
  {
    _stateMachine = stateMachine;
  }
}
