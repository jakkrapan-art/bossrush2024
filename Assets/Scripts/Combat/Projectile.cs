using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  private Action<IHitableObject> _onHit;
  private Items _source;
  private Rigidbody2D _rb;

  public void Setup(Items source, float speed, Vector2 direction, Action<IHitableObject> onHit)
  {
    _rb = GetComponent<Rigidbody2D>();
    if(_rb) _rb.freezeRotation = true;
    _onHit = onHit;
    _source = source;
    source.enabled = false;
    SetFlyVelocity(speed, direction);
  }

  private void SetFlyVelocity(float speed, Vector2 direction)
  {
    if(_rb)
    {
      _rb.AddForce(speed * direction);
      _rb.drag = 0.6f;
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
    _rb.drag = 4.2f;
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
}
