using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitableObject
{
  public bool OnHit(Items hitObj);
}