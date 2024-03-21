using UnityEngine;

[System.Serializable]
public class BossStomach
{
  public enum EatResult 
  {
    Eat = 0, NotEat = 1, Think = 2
  }
  private int eatCount = 0;

  private string _requestFood = null;
  private int MAX_PLANT_EAT = 2;

  private Product _eating = null;
  private float _startEat = 0;
  private const float DELAY_EAT = 0.05f;

  private string[] _foodList;

  public void SetupRequestFoods(Product[] products)
  {
    if (_foodList != null) throw new System.Exception("foodList already setup for BossStomatch.");

    _foodList = new string[products.Length];
    for (int i = 0; i < products.Length; i++)
    {
      Product product = products[i];
      string name = product.name.Replace("(Clone)", "");
      _foodList[i] = name;
    }
  }

  public EatResult Eat(Product product)
  {
    if (_eating != null && product == _eating && (Time.time < _startEat + DELAY_EAT))
    {
      return EatResult.NotEat;
    }

    _eating = product;
    _startEat = Time.time;
    bool increaseEatCount = true;

    if (!string.IsNullOrEmpty(_requestFood))
    {
      if(!_requestFood.Equals(product.name))
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
      eatCount++;
      Debug.Log("increase eat count, current = " + eatCount);
    }

    if (eatCount == MAX_PLANT_EAT)
    {
      RandomRequestFood();
    }
    return EatResult.Eat;
  }

  private void Digest()
  {
    Debug.Log("Digest!");
    eatCount = 0;
    _requestFood = null;
  }

  private void RandomRequestFood()
  {
    if(_foodList == null || _foodList.Length == 0)
    {
      _requestFood = "chili";
      return;
    }

    int index = Random.Range(0, _foodList.Length);
    _requestFood = _foodList[index];

    Debug.Log("I WANT TO EAT " + _requestFood.ToUpper() + "!!!");
  }
}
