using UnityEngine;

public class Soil : InteractableObject
{
  private Plant _plant;

  public bool IsAvailable() => _plant == null;

  public bool AddPlant(Plant plant)
  {
    if (!IsAvailable() || !plant) return false;
    _plant = plant;
    _coll2d.enabled = false;
    return true;
  }

  public void RemovePlant() 
  {
    if (!_plant || (_plant.GetCurrentState() is PlantMatureState == false)) return;
    _plant = null;
    _coll2d.enabled = true;
  }

  public override InteractResultData Interact(Item interactingItem)
  {
    if (interactingItem != null && _plant == null)
    {
      switch (interactingItem)
      {
        case Seed seed:
          var plant = ObjectPool.GetInstance().Get<Plant>("Plant");
          if (plant && AddPlant(plant))
          {
            seed.Use();
            _plant = plant;
            _plant.transform.SetParent(transform);
            _plant.transform.localPosition = Vector2.zero;
            _plant.Setup(seed.GetProduct(), (product) =>
            {
              product.AddOnPickedListener(() =>
              {
                RemovePlant();
                product.RemoveOnPickedListener(() =>
                {
                  RemovePlant();
                });
              });
            });
          }
          return new InteractResultData { clearHand = true };
      }
    }
    return base.Interact(interactingItem);
  }
}
