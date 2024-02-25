using UnityEngine;

public class Seed : Items, IPoolingObject
{
  [SerializeField]
  private Product _product = null;
  public Product GetProduct() => _product;

  public void Use()
  {
    ObjectPool.ReturnObjectToPool(this);
  }
}
