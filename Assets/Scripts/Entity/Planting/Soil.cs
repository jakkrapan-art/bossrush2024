using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soil : InteractOdject
{
  private Plant _plant;
  private bool _isWet;
  private bool _isFertilized;

  private Items _interactingItem;

  public bool IsAvailable() => _plant == null;

  public bool AddPlant(Plant plant)
  {
    if (!IsAvailable() || !plant) return false;
    _plant = plant;
    _coll2d.enabled = false;
    return true;
  }

  //TODO: Return the plant's product
  public void RemovePlant() 
  {
    if (!_plant || (_plant.GetCurrentState() is PlantMatureState == false)) return;
    _coll2d.enabled = true;
    //TODO: give product to player
  }

  public bool FillWater()
  {
    if (_isWet || !_plant) return false;
    _isWet = true;
    _plant.FillWater();
    return true;
  }

  public bool AddFertilizer()
  {
    if (_isFertilized || !_plant) return false;
    _isFertilized = true;
    _plant.AddFertilizer();
    return true;
  }

  public override bool CanInteract(Items itemToInteract)
  {
    if (!itemToInteract) return true;

    _interactingItem = itemToInteract;
    switch(itemToInteract)
    {
      case Seed:
      case Fertilizer:
      case WaterCan:
        return true;
      default: return false;
    }
  }

  public override void InteractResult()
  {
    if(_interactingItem)
    {
      switch(_interactingItem)
      {
        case Seed seed:
          var plant = seed.GetPlant();
          if (AddPlant(plant))
          {
            seed.Use();

            var objPool = ObjectPool.GetInstance();
            _plant = objPool.Get<Plant>(plant.name);
            _plant.transform.SetParent(transform);
            _plant.transform.localPosition = Vector2.zero;
          }
          break;
        case Fertilizer fertilizer:
          if(AddFertilizer())
          {
            fertilizer.Use();
          }
          break;
        case WaterCan waterCan:
          if(FillWater())
          {
            waterCan.Use(1);
          }
          break;
      }
    }
    else
    {
      if(_plant && _plant.GetCurrentState() is PlantMatureState)
      {
        //TODO: Add product to player hand
      }
    }

    _interactingItem = null;
  }
}
