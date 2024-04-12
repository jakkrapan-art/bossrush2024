using System;
using UnityEngine;

public class DynamicSortingOrder : MonoBehaviour
{
  [SerializeField]
  private bool _alwaysUpdate;
  [SerializeField]
  private SpriteRenderer _sr;
  [SerializeField]
  private int _updateOtherOffset = 0;
  [SerializeField]
  private DynamicSortingOrder _linkTo = null;

  private Action<int> onUpdate;

  void Start()
  {
    if(_sr == null) TryGetComponent(out _sr);
    AdjustSortingOrder();

    if(_linkTo != null)
    {
      _linkTo.SubscribeOnUpdate(SetSortingOrder);
    }
  }

  void Update()
  {
    if(_alwaysUpdate) AdjustSortingOrder();
  }

  private void OnDestroy()
  {
    if (_linkTo != null)
    {
      _linkTo.UnsubscribeOnUpdate(SetSortingOrder);
    }
  }

  public void SubscribeOnUpdate(Action<int> action)
  {
    onUpdate += action;
  }

  public void UnsubscribeOnUpdate(Action<int> action)
  {
    onUpdate -= action;
  }

  public void AdjustSortingOrder()
  {
    if(_sr == null)
    {
      _alwaysUpdate = false;
      return;
    }

    var newOrder = -Mathf.FloorToInt(transform.position.y * 100);
    SetSortingOrder(newOrder);
  }

  public void SetSortingOrder(int value)
  {
    _sr.sortingOrder = value;
    onUpdate?.Invoke(value + _updateOtherOffset);
  }

  public void SetOrderOffset(int value)
  {
    _updateOtherOffset = value;
  }

  public void SetAlwaysUpdate(bool value) 
  {
    _alwaysUpdate = value;
  }
}
