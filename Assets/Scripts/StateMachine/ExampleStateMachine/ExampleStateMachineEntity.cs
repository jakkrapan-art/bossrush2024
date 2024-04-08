using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleStateMachineEntity : Entity
{
  protected override void Awake()
  {
    base.Awake();
    _stateMachine = new ExampleStateMachine();
  }

  protected override void Update()
  {
    _stateMachine.Update();
  }

  private void FixedUpdate()
  {
    _stateMachine.FixedUpdate();
  }
}
