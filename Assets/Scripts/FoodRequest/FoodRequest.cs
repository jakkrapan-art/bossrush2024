using UnityEngine;

public class FoodRequest
{
  private PossibleFoodList _foodList;
  private FoodRecipeDB _recipeDB;

  private UIFoodRequest _ui;

  public FoodRequest(PossibleFoodList possibleFoodList, FoodRecipeDB recipeDB)
  {
    _foodList = possibleFoodList;
    _recipeDB = recipeDB;

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
      ui.Setup();
      _ui = ui;
    });
  }

  public void ShowUI(Product requestFood)
  {
    if (requestFood == null || _ui == null) return;

    FoodRecipe recipe = _recipeDB.GetRecipe(requestFood);
    if (recipe == null) return;

    _ui.ShowFoodRequest(new FoodRequestData(recipe.Food.GetIconSprite(), recipe.Materials[0].GetIconSprite(), recipe.Materials[1].GetIconSprite(), recipe.Materials[2].GetIconSprite()));
  }

  public void HideUI()
  {
    if (_ui == null) return;

    _ui.HideMaterialSlow();
  }
}
