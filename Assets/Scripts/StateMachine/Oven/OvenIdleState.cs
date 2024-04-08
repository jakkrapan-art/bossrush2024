using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenIdleState : OvenStateBase
{
  public OvenIdleState(OvenStateMachine stateMachine, Oven oven, string animationName) : base(stateMachine, oven, animationName)
  {
  }
}
