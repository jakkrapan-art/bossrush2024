using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantGrowingState : PlantStateBase
{
  private float _duration = 0;

  public PlantGrowingState(PlantStateMachine stateMachine, float duration) : base(stateMachine)
  {
    _duration = duration;
  }

  public override void OnEnter()
  {
    base.OnEnter();

    var plant = _stateMachine.GetPlant();
    _stateMachine.GetPlant().UpdateUIGrowProgressValue(0);
    plant.SetAnimatorBool("Growing", true);
    plant.SetActiveUIGrowProgress(true);
  }

  public override void OnExit()
  {
    base.OnExit();

    _stateMachine.GetPlant().SetActiveUIGrowProgress(false);
  }

  public override void Update()
  {
    base.Update();

    _stateMachine.GetPlant().UpdateUIGrowProgressValue((Time.time - _startTime)/_duration);

    if(_startTime + _duration < Time.time)
    {
      _stateMachine.ChangeState(_stateMachine.MatureState);
    }
  }
}
