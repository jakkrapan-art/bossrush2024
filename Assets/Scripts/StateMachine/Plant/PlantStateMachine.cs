using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantStateMachine : StateMachine
{
  private Plant _plant;
  public PlantFirstState FirstState { get; private set; } = null;
  public PlantGrowingState GrowingState { get; private set; } = null;
  public PlantMatureState MatureState { get; private set; } = null;

  public PlantStateMachine(Plant plant)
  {
    _plant = plant;
    FirstState = new PlantFirstState(this, ()=> { return plant.IsReadyToGrow(); });
    GrowingState = new PlantGrowingState(this, plant.GetPlantData().GrowTime);
    MatureState = new PlantMatureState(this);
    Init();
  }

  public Plant GetPlant() => _plant;

  protected override State GetInitialState()
  {
    return FirstState;
  }
}
