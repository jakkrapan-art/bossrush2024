using System.Collections.Generic;
using UnityEngine;

public class FoodRecipeDB
{
  private Dictionary<string, FoodRecipe> _recipeDictByName = new Dictionary<string, FoodRecipe>();
  private Dictionary<Product, FoodRecipe> _recipeDictByProduct = new Dictionary<Product, FoodRecipe>();

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
      if (!_recipeDictByName.ContainsKey(materialKey) && !_recipeDictByProduct.ContainsKey(recipe.Food))
      {
        _recipeDictByName.Add(materialKey, recipe);
        _recipeDictByProduct.Add(recipe.Food, recipe);
      }
      else
      {
        Debug.LogWarning("Found recipe with same materials. recipe index = " + i);
      }
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

    if(_recipeDictByName.ContainsKey(dictKey)) return _recipeDictByName[dictKey];

    return null;
  }

  public FoodRecipe GetRecipe(Product product) 
  {
    if(product == null)
    {
      return null;
    }
    else if(_recipeDictByProduct.ContainsKey(product))
    {
      return _recipeDictByProduct[product];
    }
    else
    {
      return null;
    }
  }
}
