using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
  private void Start()
  {
    _controller = new PlayerController(this);

    _controller.AddKeyAction("Pickup", () =>
    {
      //TODO: Implement for pickup action here
    }, KeyActionInputType.Press);
  }
}
