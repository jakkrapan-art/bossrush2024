using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
  public PlayerIdleState IdleState { get; private set; }
  public PlayerMoveState MoveState { get; private set; }
  public PlayerHoldState HoldState { get; private set; }

  private Player _player = null;

  public PlayerStateMachine(Player player)
  {
    _player = player;

    IdleState = new PlayerIdleState(this, player, "idle");
    MoveState = new PlayerMoveState(this, player, "move");
    HoldState = new PlayerHoldState(this, player, "hold");

    Init();
  }

  protected override State GetInitialState() => IdleState;
}
