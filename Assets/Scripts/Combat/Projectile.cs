using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  private Action<IHitableObject> _onHit;
  private Action _onStop;
  private Item _source;
  private Rigidbody2D _rb;

  public void Setup(Item source, float speed, Vector2 direction, Action<IHitableObject> onHit, Action onStop = null)
  {
    _rb = GetComponent<Rigidbody2D>();
    if(_rb) _rb.freezeRotation = true;
    _onHit = onHit;
    _source = source;
    _onStop = onStop;
    source.enabled = false;
    SetFlyVelocity(speed, direction);
  }

  private void SetFlyVelocity(float speed, Vector2 direction)
  {
    if(_rb)
    {
      _rb.AddForce(speed * direction);
      _rb.drag = 0f;
      //_rb.velocity = speed * direction * Time.fixedDeltaTime;
    }
  }

  private void OnHit(IHitableObject hitObj)
  {
    _onHit?.Invoke(hitObj);
    if (hitObj.OnHit(_source))
    {
      Destroy(this);
      ReturnObjectToPool();
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
      if(_rb.velocity == Vector2.zero)
      {
        _onStop?.Invoke();
        Destroy(this);
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
  #endregion
}
