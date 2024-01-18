using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController
{
  private Player _player = null;
  public PlayerController(Player player)
  {
    _player = player;
  }

  public override Vector2 GetMoveInput()
  {
    return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
  }
}
