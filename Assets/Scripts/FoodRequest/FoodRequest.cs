using UnityEngine;

public class FoodRequest
{
  private PossibleFoodList _foodList;

  private UIFoodRequest _ui;

  public FoodRequest(PossibleFoodList possibleFoodList)
  {
    _foodList = possibleFoodList;

    CreateUI();
  }

  public Product GetRandomFood()
  {
    float randomResult = Random.Range(0, 1f);
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
        break;
      }
    }

    return result;
  }

  private void CreateUI()
  {
    UICreator.GetInstance().CreateFoodRequestUI(ui =>
    {
      _ui = ui;
    });
  }

  public void ShowUI()
  {

  }

  public void HideUI()
  {

  }
}
