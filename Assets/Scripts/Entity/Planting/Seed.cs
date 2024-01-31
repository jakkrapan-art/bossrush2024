using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Items
{
  [SerializeField]
  private Plant _plant = null;
  public Plant GetPlant() => _plant;

  public static Seed Create(ObjectPool pool, Vector2 position)
  {
    var obj = pool.Get<Seed>("Seed");
    obj.transform.position = position;
    return obj;
  }

  public void Use()
  {
    if (gameObject.TryGetComponent(out PoolingObject pooling))
    {
      pooling.ReturnToPool();
    }
  }
}
