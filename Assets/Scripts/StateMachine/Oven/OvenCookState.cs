using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenCookState : OvenStateBase
{
  private float _cookTime;
  public OvenCookState(OvenStateMachine stateMachine, Oven oven, string animationName) : base(stateMachine, oven, animationName)
  {
  }

  public override void OnEnter()
  {
    base.OnEnter();
    _cookTime = _oven.CookTime;
  }

  public override void Update()
  {
    base.Update();
    if(Time.time >= _startTime + _cookTime)
    {
      _stateMachine.ChangeState(_stateMachine.IdleState);
    }
  }
}
