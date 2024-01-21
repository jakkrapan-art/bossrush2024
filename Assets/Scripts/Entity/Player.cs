using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Player : Entity
{
  [SerializeField]
  private GameObject _handHoldingItem;
  private Items _itemHolding;
  private Items _itemFinded;

  [SerializeField]
  private float _forceToThrow = 50;

  private void Start()
  {
    _controller = new PlayerController(this);
  }
  public void PickDropItem()
  {
    if (!_itemFinded)
    {
      if (_itemHolding)
      {
        _itemHolding.Droped();
        _itemHolding = null;
      }
    }
    else
    {
      _itemHolding = _itemFinded;
      _itemFinded.Kept(_handHoldingItem);
    }
  }

  public void ThrowItem()
  {
    if (_itemHolding)
    {
      _itemHolding.Throw(directionPlayer * _forceToThrow);
      _itemHolding = null;
    }
  }

  private void OnTriggerStay2D(Collider2D collision)
  {
    if (collision.GetComponent<Items>())
    {
      _itemFinded = collision.GetComponent<Items>();
    }
  }

  private void OnTriggerExit2D(Collider2D collision)
  {
    if (collision.GetComponent<Items>())
    {
      _itemFinded = null;
    }
  }
}
