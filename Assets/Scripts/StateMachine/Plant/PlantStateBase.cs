using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantStateBase : State
{
  protected PlantStateMachine _stateMachine = null;
  public PlantStateBase(PlantStateMachine stateMachine) 
  {
    _stateMachine = stateMachine;
  }
}
