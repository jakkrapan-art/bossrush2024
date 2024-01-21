using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityController
{
  public List<KeyActionPair> KeyActions { get; private set; } = new List<KeyActionPair>();

  public abstract Vector2 GetMoveInput();
  public void AddKeyAction(Func<bool> DoAction, Action action)
  {
    KeyActions.Add(new KeyActionPair(DoAction, action));
  }
}

public struct KeyActionPair
{
  public Func<bool> DoAction { get; }
  public Action Action { get; }
  
  public KeyActionPair(Func<bool> doAction, Action action)
  {
    DoAction = doAction;
    Action = action;
  }
}