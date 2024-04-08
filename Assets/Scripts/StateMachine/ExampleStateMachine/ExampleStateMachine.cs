using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleStateMachine : StateMachine
{
  public ExampleFirstState FirstState { get; private set; }
  public ExampleSecondState SecondState { get; private set; }
  public ExampleLastState LastState { get; private set; }

  public ExampleStateMachine()
  {
    FirstState = new ExampleFirstState(this, 3);
    SecondState = new ExampleSecondState(this, 5);
    LastState = new ExampleLastState(this);

    Init();
  }

  protected override State GetInitialState() => FirstState;
}
