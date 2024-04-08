using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSeedState : PlantStateBase
{
  private Func<bool> _readyToGrow;
  public PlantSeedState(PlantStateMachine stateMachine, Func<bool> readyToGrow) : base(stateMachine)
  {
    _readyToGrow = readyToGrow;
  }

  public override void Update()
  {
    base.Update();

    if(_readyToGrow?.Invoke() ?? false)
    {
      _stateMachine.ChangeState(_stateMachine.GrowingState);
    }
  }
}
