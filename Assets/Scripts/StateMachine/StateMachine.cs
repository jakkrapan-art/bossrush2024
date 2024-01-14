using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine
{
  private State _currentState;

  public StateMachine(State initialState) 
  {
    _currentState = initialState;
    _currentState.OnEnter();
  }

  public void ChangeState(State state)
  {
    _currentState.OnExit();
    state.OnEnter();
    _currentState = state;
  }

  public void Update() { _currentState?.Update(); }
  public void FixedUpdate() { _currentState?.FixedUpdate(); }
}
