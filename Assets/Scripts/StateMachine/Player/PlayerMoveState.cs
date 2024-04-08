using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
  public PlayerMoveState(PlayerStateMachine stateMachine, Player player, string animationName) : base(stateMachine, player, animationName)
  {
  }

  public override void Update()
  {
    base.Update();

    if(_player.GetController() != null)
    {
      if (_player.GetController().GetMoveInput().magnitude == 0)
      {
        _stateMachine.ChangeState(_stateMachine.IdleState);
      }
      else
      {
        _player.Move(_player.GetController().GetMoveInput());
      }
    }
  }

  public override void OnExit()
  {
    base.OnExit();
    _player.StopMove();
  }
}
