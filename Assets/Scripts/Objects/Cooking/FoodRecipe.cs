using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodRecipe
{
  [field: SerializeField] public Product Food { get; private set; }
  [field: SerializeField] public List<Product> Materials { get; private set; }
  [field: SerializeField] public float CookTime { get; private set; }
}
