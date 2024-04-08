using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenStateMachine : StateMachine
{
  public OvenIdleState IdleState { get; private set; }
  public OvenCookState CookState { get; private set; }

  public OvenStateMachine(Oven oven)
  {
    IdleState = new OvenIdleState(this, oven, "idle");
    CookState = new OvenCookState(this, oven, "cook");

    Init();
  }

  protected override State GetInitialState()
  {
    return IdleState;
  }
}
