using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
  public State() { }
  public virtual void OnEnter() { }
  public virtual void OnExit() { }
  public virtual void Update() { }
  public virtual void FixedUpdate() { }
}
