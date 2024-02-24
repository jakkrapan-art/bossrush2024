using UnityEngine;

public class Fertilizer : Items
{
  public void Use()
  {
    ObjectPool.ReturnObjectToPool(this);
  }
}
