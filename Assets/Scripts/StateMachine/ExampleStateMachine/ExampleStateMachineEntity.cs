using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleStateMachineEntity : Entity
{
  private void Awake()
  {
    _stateMachine = new ExampleStateMachine();
  }

  private void Update()
  {
    _stateMachine.Update();
  }

  private void FixedUpdate()
  {
    _stateMachine.FixedUpdate();
  }
}
