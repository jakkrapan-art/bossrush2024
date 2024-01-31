using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soil : InteractOdject
{
  private Plant _plant;
  private bool _isWet;
  private bool _isFertilized;

  private Items _interactingItem;

  public bool IsAvailable() => _plant;

  public bool AddPlant(Plant plant)
  {
    if (!IsAvailable()) return false;

    _plant = plant;
    return true;
  }

  //TODO: Return the plant's product
  public void RemovePlant() 
  {
    if (!_plant || (_plant.GetCurrentState() is PlantMatureState == false)) return;

    //TODO: give product to player
  }

  public bool FillWater()
  {
    if (_isWet) return false;
    _isWet = true;
    _plant.FillWater();
    return true;
  }

  public bool AddFertilizer()
  {
    if (_isFertilized) return false;
    _isFertilized = true;
    _plant.AddFertilizer();
    return true;
  }

  public override bool CanInteract(Items itemToInteract)
  {
    _interactingItem = itemToInteract;
    return (itemToInteract is Seed) || (itemToInteract == null);
  }

  public override void InteractResult()
  {
    if(_interactingItem)
    {
      switch(_interactingItem)
      {
        case Seed seed:
          if(AddPlant(seed.GetPlant()))
          {
            seed.Use();
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
