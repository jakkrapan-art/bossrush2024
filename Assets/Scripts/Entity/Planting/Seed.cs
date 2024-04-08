using UnityEngine;

public class Seed : Item, IPoolingObject
{
  [SerializeField]
  private Product _product = null;
  public Product GetProduct() => _product;

  public void Use()
  {
    ObjectPool.ReturnObjectToPool(this);
  }
}
