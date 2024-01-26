using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine
{
  public State CurrentState { get; protected set; } = null;

  public void Init()
  {
    CurrentState = GetInitialState();
    CurrentState.OnEnter();
  }

  public void ChangeState(State state)
  {
    CurrentState.OnExit();
    state.OnEnter();
    CurrentState = state;
  }

  protected abstract State GetInitialState();

  public void Update() { CurrentState?.Update(); }
  public void FixedUpdate() { CurrentState?.FixedUpdate(); }
}
