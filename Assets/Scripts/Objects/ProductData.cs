using UnityEngine;
[CreateAssetMenu(fileName = "ProductData", menuName = "Data/ProductData")]

public class ProductData : ItemData
{
  [field: SerializeField] public bool Eatable { get; private set; }
  [field: SerializeField] public bool Material { get; private set; }
}
