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
          var plant = ObjectPool.GetInstance().Get<Plant>("Plant");
          if (plant && AddPlant(plant))
          {
            seed.Use();
            _plant = plant;
            _plant.transform.SetParent(transform);
            _plant.transform.localPosition = Vector2.zero;
            _plant.Setup(seed.GetProduct());
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

    _interactingItem = null;
  }
}
