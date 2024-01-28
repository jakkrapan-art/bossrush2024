using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soil : MonoBehaviour
{
  private Plant _plant;
  private bool _isWet;
  private bool _isFertilized;
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
    return true;
  }
}
