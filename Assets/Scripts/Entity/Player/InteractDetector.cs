using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractDetector : MonoBehaviour
{
  [SerializeField]
  private Collider2D _playerCollider;

  [SerializeField] private float _detectRange = 1.5f;
  [SerializeField] private Vector3 _positionOffset = Vector3.zero;
  private int _delayFrameRate = 5;

  private InteractableObject _target = null;
  public Item GetDetectedItem()
  {
    if(_target is Item targetItem) return targetItem;
    else return null;
  }
  public InteractableObject GetInteractObject() => _target;

  private void Update()
  {
    if(Time.frameCount % _delayFrameRate == 0)
    {
      Scan();
    }
  }

  private void Scan()
  {
    SetActivePlayerCollider(false);
    var hit = Physics2D.OverlapCircleAll(transform.position, _detectRange);
    InteractableObject nearestInteractObj = null;
    float nearestDistance = 0;
    foreach(var detectedObj in hit) 
    {
      if(detectedObj.TryGetComponent(out InteractableObject obj))
      {
        if (!obj.enabled) continue;

        float itemDistance = Vector2.Distance(GetPosition(), obj.transform.position);
        if (!nearestInteractObj || (itemDistance < nearestDistance))
        {
          nearestInteractObj = obj;
          nearestDistance = itemDistance;
        }
      }
    }

    _target = nearestInteractObj;
    SetActivePlayerCollider(true);
  }

  private void SetActivePlayerCollider(bool active)
  {
    if(_playerCollider)
    {
      _playerCollider.enabled = active;
    }
  }

  private Vector3 GetPosition() => transform.position + _positionOffset;

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(GetPosition(), _detectRange);
  }
}
