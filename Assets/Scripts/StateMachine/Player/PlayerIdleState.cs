using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
  public PlayerIdleState(PlayerStateMachine stateMachine, Player player, string animationName) : base(stateMachine, player, animationName)
  {
  }

  public override void Update()
  {
    base.Update();

    if(_player.GetController().GetMoveInput().magnitude > 0 ) { _stateMachine.ChangeState(_stateMachine.MoveState); }
  }
}
