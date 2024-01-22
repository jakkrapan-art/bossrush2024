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

    Debug.Log("enter mature state.");
    _stateMachine.GetPlant().SetAnimatorBool("Mature", true);
  }
}
