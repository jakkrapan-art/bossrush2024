using System;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
  [SerializeField]
  protected string _name;
  protected float _activeTime;
  [SerializeField, Min(0)]
  protected float _cooldownTime;

  public virtual void Setup() 
  {
    _activeTime = 0;
  }
  public virtual void Activate()
  {
    _activeTime = Time.time;
  }

  public bool isReady => _activeTime == 0 || Time.time > _activeTime + _cooldownTime;
}
