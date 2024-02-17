
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Oven : InteractOdject
{
  private bool _isCooking = false;
  private float _currentTimer;
  private int _currentItemRow;

  [SerializeField] private List<RecipeFoods> _listRecipeFoods;
  [SerializeField] private List<ItemData> _listItemsInput;
  [SerializeField] private Image[] IconItemInput_UI;

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
    if (IconItemInput_UI.Length > _currentItemRow && itemToInteract != null)
    {
      AddItemInput(itemToInteract);
      return true;
    }
    else
    {
      cooking();
      return false;
    }
  }
  public override void InteractResult()
  {
    Debug.Log("get new item");
  }
  private void cooking()
  {
    _isCooking = true;
  }

  private void AddItemInput(Items itemToInteract)
  {
    Destroy(itemToInteract.gameObject);
    _listItemsInput.Add(itemToInteract.GetItemData());
    IconItemInput_UI[_currentItemRow].sprite = itemToInteract.GetIconItem();
    _currentItemRow++;
  }

  private void cookingFinish()
  {
    /*foreach (RecipeFoods recipeFood in _listRecipeFoods)
    {
      foreach (ItemData itemsRequest in recipeFood.ItemsInput)
      {
        foreach (ItemData itemsInput in _listItemsInput)
        {
          if (itemsRequest == itemsInput)
          {

          }
        }
      }
    }*/
    foreach (RecipeFoods recipeFood in _listRecipeFoods)
    {
      if (recipeFood.ItemsInput == _listItemsInput)
      {
        _ItemsOutput = Instantiate(recipeFood.ItemOutput, resultPos.transform.position, Quaternion.identity);
      }
    }
    _currentItemRow = 0;
    _listItemsInput = null;
    foreach (Image icon in IconItemInput_UI)
    {
      icon.sprite = null;

    }
    _isCooking = false;
  }
}
