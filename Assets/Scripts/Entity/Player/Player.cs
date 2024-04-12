using System;
using System.Collections;
using UnityEngine;

public class Player : Entity
{
  [SerializeField]
  private GameObject _handHoldingItem;
  [SerializeField]
  private Transform _dropItemPosition;
  [SerializeField]
  private Transform _throwItemPoint;
  private Item _holdingItem;
  private InteractableObject _itemFinded;
  private InteractableObject _interactingObject;

  [SerializeField]
  private DynamicSortingOrder _dynamicSortingOrder = null;
  private DynamicSortingOrder _sortingOrderLinked = null;

  private bool _isInteraction;
  private float _currentTimer;
  private float _timeToInteraction;

  [SerializeField]
  private InteractDetector _objDetector = default;

  private Animator _anim;

  public Animator GetAnimator() => _anim;
  public PlayerController GetController() => (PlayerController)_controller;

  protected override void Awake()
  {
    base.Awake();
    TryGetComponent(out _anim);
  }

  private void Start()
  {
    _controller = new PlayerController(this);
    _stateMachine = new PlayerStateMachine(this);
  }

  protected override void Update()
  {
    base.Update();
    if (_stateMachine != null) _stateMachine.Update();
    if (_isInteraction)
    {
      StopMove();
      _currentTimer += Time.deltaTime;
      if (_currentTimer >= _timeToInteraction)
      {
        InteractFinish();
      }
    }
  }

  protected void FixedUpdate()
  {
    if (_stateMachine != null) _stateMachine.FixedUpdate();
  }

  public void PickOrDrop()
  {
    if (!_objDetector) return;
    var targetItem = _objDetector.GetDetectedItem();
    if(targetItem) 
    {
      PickItem(targetItem);
    }
    else
    {
      DropItem();
    }
  }

  private Action<int> _updateLinkedSortingOrderValueCallback;

  public void PickItem(Item item)
  {
    if (!item) return;

    DropItem();

    item.Kept(_handHoldingItem);
    _holdingItem = item;

    if (_dynamicSortingOrder != null && item.TryGetComponent(out _sortingOrderLinked))
    {
      _updateLinkedSortingOrderValueCallback = (newVal) =>
      {
        if (_sortingOrderLinked != null)
        {
          _sortingOrderLinked.SetSortingOrder(newVal + 1);
        }
      };

      _dynamicSortingOrder.SubscribeOnUpdate(_updateLinkedSortingOrderValueCallback);
    }
  }

  public void DropItem()
  {
    if (!_holdingItem) return;

    RemoveSortingOrderLink();

    if (_dropItemPosition) _holdingItem.Drop(_dropItemPosition.position);
    _holdingItem = null;
  }

  public void ThrowItem()
  {
    RemoveSortingOrderLink();

    if (_holdingItem)
    {
      _holdingItem.Throw(_throwItemPoint.position, directionPlayer, Const.THROW_FORCE);
      _holdingItem = null;
    }
  }

  private void RemoveSortingOrderLink()
  {
    if (_dynamicSortingOrder != null && _sortingOrderLinked != null && _updateLinkedSortingOrderValueCallback != null)
    {
      _dynamicSortingOrder.UnsubscribeOnUpdate(_updateLinkedSortingOrderValueCallback);
      _sortingOrderLinked.AdjustSortingOrder();
      _sortingOrderLinked = null;
    }
  }

  public void StartInteractObject()
  {
    if (!_objDetector) return;
    var interactTarget = _objDetector.GetInteractObject();
    if (interactTarget != null && !_isInteraction)
    {
      var result = interactTarget.Interact(_holdingItem);
      if (result.clearHand)
      {
        _holdingItem.ReturnToPool();
        _holdingItem = null;
      }

      if (result.waitTime > 0)
      {
        _isInteraction = true;
        _timeToInteraction = result.waitTime;
        _interactingObject = interactTarget;
        _currentTimer = 0;
        SetEnableMove(false);
      }

      if(result.returnItem != null)
      {
        PickItem(result.returnItem);
      }

      if(result.waitResult != null)
      {
        SetEnableMove(false);
        StartCoroutine(WaitForInteractResult(result.waitResult));
      }
    }
  }

  private IEnumerator WaitForInteractResult(Func<InteractableObject.WaitResultData> func)
  {
    if (func == null) yield break;
    InteractableObject.WaitResultData result = default;
    yield return new WaitUntil(()=> 
    {
      result = func.Invoke();
      return result.finish;
    });

    if(result.resultItem != null)
    {
      PickItem(result.resultItem);
    }

    SetEnableMove(true);
  }

  public void InteractFinish()
  {
    if (_interactingObject && _isInteraction)
    {
      _isInteraction = false;
      _interactingObject = null;
    }
    SetEnableMove(true);
  }
}
