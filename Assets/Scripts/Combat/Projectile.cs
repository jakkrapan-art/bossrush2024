using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  private Action<IHitableObject> _onHit;
  private Action _onStop;
  private Item _source;
  private Rigidbody2D _rb;
  private bool _isMoved = false;
  private Vector2 _startPos;

  public void Setup(Item source, float speed, Vector2 direction, Action<IHitableObject> onHit, Action onStop = null)
  {
    if(TryGetComponent(out _rb))
    {
      _rb.freezeRotation = true;
      _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    _startPos = transform.position;
    _onHit = onHit;
    _source = source;
    _onStop = onStop;
    //source.enabled = false;
    SetFlyVelocity(speed, direction);
  }

  private void SetFlyVelocity(float speed, Vector2 direction)
  {
    if(_rb)
    {
      _rb.AddForce(speed * direction);
      _rb.drag = 0f;
    }
  }

  private void OnHit(IHitableObject hitObj)
  {
    _onHit?.Invoke(hitObj);
    if (hitObj.OnHit(_source))
    {
      ReturnObjectToPool();
      Destroy(this);
    }
    else
    {
      Knockback();
    }
  }

  private void ReturnObjectToPool()
  {
    if (_source)
    {
      _source.enabled = true;
      _source.ReturnToPool();
    }
  }

  private void Knockback()
  {
    if (!_rb) return;

    var veloTemp = _rb.velocity;
    _rb.velocity = Vector3.zero;
    _rb.velocity = veloTemp * -1;
    _rb.drag += 3.5f;
  }

  #region Unity Functions
  private void Update()
  {
    if (Time.frameCount % 5 != 0) return;

    if(_rb != null)
    {
      _rb.drag += 0.08f;

      if (_isMoved && _rb.velocity == Vector2.zero)
      {
        _onStop?.Invoke();
        Destroy(this);
      }

      if (!_isMoved)
      {
        _isMoved = Mathf.Abs(Vector2.Distance(_startPos, transform.position)) > 0;
      }
    }
  }

  private void OnTriggerEnter2D(Collider2D otherCollider)
  {
    if (otherCollider.gameObject.TryGetComponent(out IHitableObject hitable))
    {
      OnHit(hitable);
    }
    else if (!otherCollider.transform.root.GetComponent<Player>() && !otherCollider.isTrigger)
    {
      Knockback();
    }
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.collider.TryGetComponent(out IHitableObject hitable))
    {
      OnHit(hitable);
    }
    else if (!collision.transform.root.GetComponent<Player>() && !collision.collider.isTrigger)
    {
      Knockback();
    }
  }

  private void OnDestroy()
  {
    if(_source != null) _source.enabled = true;
  }
  #endregion
}
