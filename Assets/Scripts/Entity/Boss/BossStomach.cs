using System;
using UnityEngine;

[System.Serializable]
public class BossStomach
{
  public enum EatResult 
  {
    Eat = 0, NotEat = 1, Think = 2
  }
  private int _eatCount = 0;

  private string _desireFood = null;
  private int _maxPlantEat = 2;

  private Product _eating = null;
  private float _startEat = 0;
  private const float DELAY_EAT = 0.05f;

  private FoodRequest _foodRequest = null;

  public BossStomach(int maxPlantEat)
  {
    _maxPlantEat = maxPlantEat;
  }

  public void Setup(PossibleFoodList possibleFoodList, FoodRecipeDB recipeDB)
  {
    if (_foodRequest != null) throw new System.Exception("foodList already setup for BossStomatch.");
    _foodRequest = new FoodRequest(possibleFoodList, recipeDB);
  }

  public EatResult Eat(Product product)
  {
    if ((_eatCount == _maxPlantEat && string.IsNullOrEmpty(_desireFood)) || (_eating != null && product == _eating && (Time.time < _startEat + DELAY_EAT)))
    {
      return EatResult.NotEat;
    }

    _eating = product;
    _startEat = Time.time;
    bool increaseEatCount = true;

    if (!string.IsNullOrEmpty(_desireFood))
    {
      if(!_desireFood.Equals(product.name))
      {
        return EatResult.NotEat;
      }
      else
      {
        Digest();
        increaseEatCount = false;
      }
    }

    if(increaseEatCount)
    {
      _eatCount++;

      if (_eatCount >= _maxPlantEat)
      {
        return EatResult.Think;
      }
    }

    return EatResult.Eat;
  }

  private void Digest()
  {
    _eatCount = 0;
    _desireFood = null;

    _foodRequest.HideUI();
  }

  public void RandomRequestFood(Action<Product> callback)
  {
    if (_foodRequest == null) throw new System.Exception("Cannot call RandomRequestFood if not setup yet");

    Product randomResult = _foodRequest.GetRandomFood();

    if(randomResult == null) 
    { 
      _desireFood = "chili"; 
    }
    else
    {
      _desireFood = randomResult.name;
    }

    Debug.LogWarning("I WANT TO EAT " + _desireFood + "!!!");
    _foodRequest.ShowUI(randomResult);
    callback?.Invoke(randomResult);
  }
}
