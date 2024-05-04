using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Const
{
  public static string GAME_SCENE_NAME = "SampleScene";

  //Item
  public static float ITEM_LIFETIME = 30f;
  public static float THROW_FORCE = 500f;

  //Skill
  public static float SKILL_COOLTIME = 10f;

  private static readonly Dictionary<Type, string> OBJECT_PREFIX = new Dictionary<Type, string>()
  {
    { typeof(Product), "Product/"}
  };

  public static string GetObjectPrefix(Type type)
  {
    if (OBJECT_PREFIX.ContainsKey(type))
    {
      return OBJECT_PREFIX[type];
    }
    else
    {
      return "";
    }
  }
}
