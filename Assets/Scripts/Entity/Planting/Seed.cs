using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Items
{
  [SerializeField]
  private Items _product = null;
  public Items GetProduct() => _product;
  public static Seed Create(string name, Vector2 position)
  {
    var pool = ObjectPool.GetInstance();
    var obj = pool.Get<Seed>(name + "_seed");
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
