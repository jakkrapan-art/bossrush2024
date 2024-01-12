using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityController
{
  public Dictionary<KeyCode, Action> KeyActions { get; private set; } = new Dictionary<KeyCode, Action>();

  public abstract Vector2 GetMoveInput();
  public void AddKeyAction(KeyCode key, Action action)
  {
    KeyActions.Add(key, action);
  }
}
