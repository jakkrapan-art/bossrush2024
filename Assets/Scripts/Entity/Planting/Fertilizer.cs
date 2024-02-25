using UnityEngine;

public class Fertilizer : Items, IPoolingObject
{

  public void Use()
  {
    ObjectPool.ReturnObjectToPool(this);
  }
}
