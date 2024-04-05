using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Food Recipe List", menuName = "Data/FoodRecipeList")]
public class FoodRecipeList : ScriptableObject
{
  [field: SerializeField]public FoodRecipe[] Recipes { get; private set; } = default;
}
