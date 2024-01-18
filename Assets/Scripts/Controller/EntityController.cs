using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityController
{
  public List<KeyActionPair> KeyActions { get; private set; } = new List<KeyActionPair>();

  public abstract Vector2 GetMoveInput();
  public void AddKeyAction(string key, Action action, KeyActionInputType inputType)
  {
    KeyActions.Add(new KeyActionPair(key, action, inputType));
  }
}

public struct KeyActionPair
{
  public string KeyName { get; }
  public Action Action { get; }
  public KeyActionInputType InputType { get; }

  public KeyActionPair(string key, Action action, KeyActionInputType inputType)
  {
    KeyName = key;
    Action = action;
    InputType = inputType;
  }
}

public enum KeyActionInputType
{
  Press, Hold
}