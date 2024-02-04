using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHoldState : PlayerStateBase
{
  public PlayerHoldState(PlayerStateMachine stateMachine, Player player, string animationName) : base(stateMachine, player, animationName)
  {
  }
}
