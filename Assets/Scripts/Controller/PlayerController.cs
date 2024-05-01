using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController
{
  private Player _player = null;
  public PlayerController(Player player)
  {
    _player = player;

    AddKeyAction(()=> { return Input.GetButtonDown("PickupDrop"); }, () =>
    {
      _player.PickOrDrop();
    });

    AddKeyAction(() => { return Input.GetButtonDown("Throw"); }, () =>
    {
      _player.ThrowItem();

    });

    AddKeyAction(() => { return Input.GetButtonDown("Interact"); }, () =>
    {
      _player.StartInteractObject();
    });

    AddKeyAction(() => { return Input.GetKeyDown(KeyCode.Space); }, () =>
    {
      Utility.Shake(_player.gameObject);
    });
  }

  public override Vector2 GetMoveInput()
  {
    return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
  }
}
