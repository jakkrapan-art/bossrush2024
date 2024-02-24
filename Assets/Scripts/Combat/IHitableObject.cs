using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitableObject
{
  public bool TakeDamage(Items hitter, float damage);
}
