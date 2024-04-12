using System.Collections.Generic;
using UnityEngine;

public class FoodRecipeDB
{
  private Dictionary<string, FoodRecipe> recipeDict = new Dictionary<string, FoodRecipe>();

  public FoodRecipeDB(FoodRecipe[] recipes)
  {
    Setup(recipes);
  }

  private void Setup(FoodRecipe[] recipes)
  {
    for (int i = 0; i < recipes.Length; i++)
    {
      var recipe = recipes[i];
      List<string> materialNames = new List<string>();
      foreach (var mat in recipe.Materials)
      {
        if (mat == null) continue;
        materialNames.Add(mat.name);
      }

      materialNames.Sort();
      string materialKey = string.Join("", materialNames);
      if (!recipeDict.ContainsKey(materialKey))
        recipeDict.Add(materialKey, recipe);
      else 
        Debug.LogWarning("Found recipe with same materials. recipe index = " + i);
    }
  }

  public FoodRecipe GetRecipe(Product[] materials)
  {
    List<string> materialNames = new List<string>();
    foreach (var mat in materials)
    {
      if (mat == null) continue;

      materialNames.Add(mat.name);
    }

    materialNames.Sort();
    string dictKey = string.Join("", materialNames);

    if(recipeDict.ContainsKey(dictKey)) return recipeDict[dictKey];

    return null;
  }
}
