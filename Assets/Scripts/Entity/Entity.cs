using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour
{
  protected Rigidbody2D _rb;
  protected EntityController _controller;
  [SerializeField]
  protected EntityData _data;

  protected StateMachine _stateMachine = null;
  protected Vector2 directionPlayer;
  protected bool _enableMove = true;
  public void SetEnableMove(bool enable)
  {
    Debug.Log("set enable move to " + enable);
    _enableMove = enable;
  }

  private bool _isFacingRight = true;
  #region Unity Functions
  protected virtual void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
  }

  protected virtual void Update()
  {
    if (_controller != null)
    {
      foreach (var action in _controller.KeyActions)
      {
        if (action.DoAction?.Invoke() ?? false) action.Action?.Invoke();
      }
    }
  }
  #endregion

  public void Move(Vector2 direction)
  {
    if (!_enableMove) return;

    if (_rb)  _rb.velocity = direction * _data.MoveSpeed * Time.fixedDeltaTime;
    if (direction != Vector2.zero) directionPlayer = direction;

    if(direction.x < 0 && _isFacingRight)
    {
      FlipX();
    }
    else if(direction.x > 0 && !_isFacingRight)
    {
      FlipX();
    }
  }

  private void FlipX()
  {
    transform.Rotate(new Vector2(0, 180));
    _isFacingRight = !_isFacingRight;
  }

  public void StopMove()
  {
    Move(Vector2.zero);
  }
}
