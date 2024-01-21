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
      _player.PickDropItem();
    });

   /* AddKeyAction(Input.GetButtonDown("Drop"), () =>
    {
      _player.DropItem();

    });*/

    AddKeyAction(() => { return Input.GetButtonDown("Throw"); }, () =>
    {
      Debug.Log("throw");
      _player.ThrowItem();

    });

    AddKeyAction(() => { return Input.GetButtonDown("Interact"); }, () =>
    {
      //TODO: Implement for pickup action here
    });
  }

  public override Vector2 GetMoveInput()
  {
    return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
  }
}
