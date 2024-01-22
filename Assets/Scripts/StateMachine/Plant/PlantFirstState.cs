using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantFirstState : PlantStateBase
{
  private Func<bool> _readyToGrow;
  public PlantFirstState(PlantStateMachine stateMachine, Func<bool> readyToGrow) : base(stateMachine)
  {
    _readyToGrow = readyToGrow;
  }

  public override void Update()
  {
    base.Update();

    if(_readyToGrow?.Invoke() ?? false)
    {
      Debug.Log("Plant ready to grow changing to grow state");
      _stateMachine.ChangeState(_stateMachine.GrowingState);
    }
  }
}
