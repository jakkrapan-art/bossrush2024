using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenStateBase : State
{
  protected Oven _oven = null;
  protected OvenStateMachine _stateMachine = null;
  protected string _animationName = string.Empty;
  public OvenStateBase(OvenStateMachine stateMachine, Oven oven, string animationName)
  {
    _stateMachine = stateMachine;
    _oven = oven;
    _animationName = animationName;
  }

  public override void OnEnter()
  {
    base.OnEnter();
    ChangeBoolValue(true);
  }

  public override void OnExit()
  {
    base.OnExit();
    ChangeBoolValue(false);
  }

  private void ChangeBoolValue(bool value)
  {
    var animator = _oven?.GetAnimator() ?? null;
    if (animator)
    {
      animator.SetBool(_animationName, value);
    }
  }
}
