using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
  private Rigidbody2D _rb;
  private EntityController _controller;

  #region Unity Functions
  private void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
  }

  private void Update()
  {
    if(_controller != null)
    {
      foreach(var action in _controller.KeyActions)
      {
        if (Input.GetKeyDown(action.Key)) action.Value?.Invoke();
      }
    }
  }
  #endregion

  public void Move(Vector2 direction)
  {
    if(_rb) _rb.velocity = direction;
  }

  public void StopMove()
  {
    Move(Vector2.zero);
  }
}
