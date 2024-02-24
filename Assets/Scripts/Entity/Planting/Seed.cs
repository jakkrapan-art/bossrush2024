using UnityEngine;

public class Seed : Items
{
  [SerializeField]
  private Items _product = null;
  public Items GetProduct() => _product;

  public void Use()
  {
    ObjectPool.ReturnObjectToPool(this);
  }
}
