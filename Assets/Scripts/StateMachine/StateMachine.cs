using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine
{
  private State _currentState;

  public void Init()
  {
    _currentState = GetInitialState();
    _currentState.OnEnter();
  }

  public void ChangeState(State state)
  {
    _currentState.OnExit();
    state.OnEnter();
    _currentState = state;
  }

  protected abstract State GetInitialState();

  public void Update() { _currentState?.Update(); }
  public void FixedUpdate() { _currentState?.FixedUpdate(); }
}
