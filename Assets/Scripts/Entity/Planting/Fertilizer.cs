using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fertilizer : Items
{
  public static Fertilizer Create(ObjectPool pool, Vector2 position)
  {
    var obj = pool.Get<Fertilizer>("Fertilizer");
    obj.transform.position = position;
    return obj;
  }

  public void Use()
  {
    if(gameObject.TryGetComponent(out PoolingObject pooling))
    {
      pooling.ReturnToPool();
    }
  }
}
