using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateBase : State
{
  protected PlayerStateMachine _stateMachine = null;
  protected Player _player = null;
  protected string _animationName = string.Empty;
  public PlayerStateBase(PlayerStateMachine stateMachine, Player player, string animationName)
  {
    _stateMachine = stateMachine;
    _player = player;
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
    var anim = _player?.GetAnimator() ?? null;
    if (anim)
    {
      anim.SetBool(_animationName, value);
    }
  }
}
