using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
  protected float _startTime = 0;
  public bool SavePrevious { get; } = false;

  public State(bool savePrevious = false) 
  { 
    SavePrevious = savePrevious;
  }

  public virtual void OnEnter() 
  {
    _startTime = Time.time;
  }
  public virtual void OnExit() { }
  public virtual void Update() { }
  public virtual void FixedUpdate() { }
}