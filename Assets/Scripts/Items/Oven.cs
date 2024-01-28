using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : InteractOdject
{
  private bool _isCooking = false;
  private float _currentTimer;

  private List<RecipeFoods> _listRecipeFoods;
  private List<Items> _listItemsInput;


  [SerializeField] private GameObject resultPos;

  private void Update()
  {
    if (_isCooking)
    {
      _currentTimer += Time.deltaTime;
      if (_currentTimer >= _timeToInteract)
      {
        cookingFinish();
      }
    }

  }

  public override bool CanInteract(Items itemToInteract)
  {
    if (_isCooking) { return false; }

    if (_ItemsOutput)
    {

      return false;
    }

    if (_listItemsInput.Count < 3)
    {
      _listItemsInput.Add(itemToInteract);

      return true;
    }
    else
    {
      cooking();
      return false;
    }
  }

  private void cooking()
  {
    _isCooking = true;
  }

  private void cookingFinish()
  {
    foreach (RecipeFoods recipeFood in _listRecipeFoods)
    {
      if (recipeFood.ItemsInput == _listItemsInput)
      {
        _ItemsOutput = Instantiate(recipeFood.ItemOutput, resultPos.transform.position, Quaternion.identity);
      }
    }
    _isCooking = false;
  }
}
