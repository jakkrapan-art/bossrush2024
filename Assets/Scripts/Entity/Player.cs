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
  private Items _itemInteraction;

  [SerializeField]
  private float _forceToThrow = 50;
  private bool _isInteraction;
  private float _currentTimer;
  private float _timeToInteraction;

  private void Start()
  {
    _controller = new PlayerController(this);
  }

  private void Update()
  {
    if (_isInteraction)
    {
      //StopMove();
      _currentTimer += Time.deltaTime;
    }
    if (_currentTimer >= _timeToInteraction)
    {
      InteractFinish();
    }
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
  public void StartInteractOject()
  {
    if (!_itemFinded && _itemFinded.GetComponent<InteractOdject>() && !_isInteraction)
    {
      if (_itemFinded.CanInteract(_itemHolding))
      {
        _isInteraction = true;
        _timeToInteraction = _itemFinded.GetTimeToInteract();
        _itemInteraction = _itemFinded;
        _currentTimer = 0;
      }
    }
  }
  public void InteractFinish()
  {
    if (_itemInteraction && _isInteraction)
    {
      _isInteraction = false;
      _itemInteraction.InteractResult();
      _itemInteraction = null;
    }
  }

  public void CancelInteractOject()
  {
    if (!_itemFinded && _isInteraction)
    {
      _isInteraction = false;
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
