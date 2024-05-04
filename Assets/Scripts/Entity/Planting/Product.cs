using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : Item
{
  public int GetDamage() { return 5; }
  public ProductData GetProductData() => _ItemData as ProductData;
}
