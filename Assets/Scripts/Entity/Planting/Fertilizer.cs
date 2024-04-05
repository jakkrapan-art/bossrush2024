using UnityEngine;

public class Fertilizer : Item, IPoolingObject
{

  public void Use()
  {
    ObjectPool.ReturnObjectToPool(this);
  }
}
