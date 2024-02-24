using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  private float _damage = 0;
  private Action<IHitableObject> _onHit;
  private Items _source;
  private Rigidbody2D _rb;

  public void Setup(Items source, float speed, Vector2 direction, Action<IHitableObject> onHit)
  {
    _rb = GetComponent<Rigidbody2D>();
    _onHit = onHit;
    _source = source;
    source.enabled = false;
    SetFlyVelocity(speed, direction);
  }

  private void SetFlyVelocity(float speed, Vector2 direction)
  {
    if(_rb) _rb.velocity = speed * direction * Time.fixedDeltaTime;
  }

  private void OnHit(IHitableObject hitObj)
  {
    _onHit?.Invoke(hitObj);
    if (hitObj.TakeDamage(_source, _damage))
    {
      Destroy(this);
      _source.enabled = true;
      _source.ReturnToPool();
    }
  }

  private void OnTriggerEnter2D(Collider2D otherCollider)
  {
    if (otherCollider.gameObject.TryGetComponent(out IHitableObject hitable))
    {
      OnHit(hitable);
    }
  }
}
