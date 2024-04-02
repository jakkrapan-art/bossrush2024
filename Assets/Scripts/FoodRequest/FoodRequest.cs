using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class FoodRequest
{
  private PossibleFoodList _foodList;
  private Dictionary<float, Product> _foodMapCache = new Dictionary<float, Product>();
  public FoodRequest(PossibleFoodList possibleFoodList)
  {
    _foodList = possibleFoodList;
  }

  public Product GetRandomFood()
  {
    float randomResult = Random.Range(0, 1f);
    if (_foodMapCache.ContainsKey(randomResult)) return _foodMapCache[randomResult];
    Product result = null;
    float currentPossibility = 0f;
    for (int i = 0; i < _foodList.PossibleFoods.Length; i++)
    {
      currentPossibility += _foodList.PossibleFoods[i].possibility;

      if (i == _foodList.PossibleFoods.Length - 1 ||
        (i == 0 && randomResult < currentPossibility) ||
        (randomResult == currentPossibility) ||
        (i > 0 && currentPossibility > randomResult && randomResult > currentPossibility - _foodList.PossibleFoods[i - 1].possibility))
      {
        result = _foodList.PossibleFoods[i].product;
        SaveCache(result, randomResult);
        break;
      }
    }

    return result;
  }

  private void SaveCache(Product product, float randomResult) 
  {
    if (product == null || _foodMapCache.ContainsKey(randomResult)) return;

    _foodMapCache.Add(randomResult, product);
  }

  private void CreateUI()
  {

  }
}
