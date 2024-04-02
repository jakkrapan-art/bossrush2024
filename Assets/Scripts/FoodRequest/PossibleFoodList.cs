using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Possible Food List", menuName = "Data/PossibleFoodList")]
public class PossibleFoodList : ScriptableObject
{
  [field: SerializeField] public PossibleFood[] PossibleFoods { get; private set; }
}

[System.Serializable]
public class PossibleFood
{
  [field: SerializeField] public Product product { get; private set; }
  [field: Range(0, 1), SerializeField]
  public float possibility { get; private set; }

  public PossibleFood(Product product, int possibility)
  {
    this.product = product;
    this.possibility = possibility;
  }
}
