using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityController
{
  public List<KeyActionPair> KeyActions { get; private set; } = new List<KeyActionPair>();

  public abstract Vector2 GetMoveInput();
  public void AddKeyAction(KeyCode key, Action action)
  {
    KeyActions.Add(new KeyActionPair(key, action));
  }
}

public struct KeyActionPair
{
  public KeyCode Key { get; }
  public Action Action { get; }

  public KeyActionPair(KeyCode key, Action action)
  {
    Key = key;
    Action = action;
  }
}