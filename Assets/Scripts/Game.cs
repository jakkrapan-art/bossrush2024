using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
  [Header("Cooking System")]
  [SerializeField]
  private FoodRecipeList recipeList = null;
  //database obj
  private FoodRecipeDB recipeDB = null;

  [SerializeField]
  private Oven ovenTemplate = default;
  [SerializeField]
  private Vector3 ovenPos = default;

  public void Setup()
  {
    recipeDB = new FoodRecipeDB(recipeList.Recipes);

    //create oven
    var oven = Instantiate(ovenTemplate, ovenPos, Quaternion.identity);
    oven.Setup(recipeDB);
  }
}
