using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMatureState : PlantStateBase
{
  public PlantMatureState(PlantStateMachine stateMachine) : base(stateMachine)
  {
  }

  public override void OnEnter()
  {
    base.OnEnter();

    var plant = _stateMachine.GetPlant();
    plant.SetAnimatorBool("Mature", true);
    plant.OnFullyGrowth();
  }
}
