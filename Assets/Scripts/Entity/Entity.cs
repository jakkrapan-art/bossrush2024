using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
  private Rigidbody2D _rb;
  protected EntityController _controller;
  [SerializeField]
  private EntityData _data;

  protected StateMachine _stateMachine = null;

  #region Unity Functions
  private void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
  }

  private void Update()
  {
    if(_controller != null)
    {
      Move(_controller.GetMoveInput());

      foreach(var action in _controller.KeyActions)
      {
        if (action.DoAction) action.Action?.Invoke();
      }
    }
  }
  #endregion

  public void Move(Vector2 direction)
  {
    if(_rb) _rb.velocity = direction * _data.MoveSpeed * Time.fixedDeltaTime;
  }

  public void StopMove()
  {
    Move(Vector2.zero);
  }
}
