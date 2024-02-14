using UnityEngine;

public class Player : Entity
{
  [SerializeField]
  private GameObject _handHoldingItem;
  [SerializeField]
  private Transform _dropItemPosition;
  private Items _holdingItem;
  private InteractOdject _itemFinded;
  private InteractOdject _interactingObject;

  [SerializeField]
  private float _forceToThrow = 50;
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

  public void PickItem(Items item)
  {
    if (!item || item is Plant) return;

    DropItem();

    item.Kept(_handHoldingItem);
    _holdingItem = item;
  }

  public void DropItem()
  {
    if (!_holdingItem) return;

    if(_dropItemPosition) _holdingItem.Drop(_dropItemPosition.position);
    _holdingItem = null;
  }

  public void ThrowItem()
  {
    if (_holdingItem)
    {
      _holdingItem.Throw(directionPlayer * _forceToThrow);
      _holdingItem = null;
    }
  }
  public void StartInteractOject()
  {
    if (!_objDetector) return;
    var interactTarget = _objDetector.GetInteractOdject();
    if (interactTarget && !_isInteraction && interactTarget.CanInteract(_holdingItem))
    {
      _isInteraction = true;
      _timeToInteraction = interactTarget.GetTimeToInteract();
      _interactingObject = interactTarget;
      _currentTimer = 0;
      Debug.Log("StartInteractOject");
    }
  }
  public void InteractFinish()
  {
    Debug.Log("interact finish");
    if (_interactingObject && _isInteraction)
    {
      _isInteraction = false;
      _interactingObject.InteractResult();
      _interactingObject = null;
      Debug.Log("InteractFinish");
    }
  }

  public void CancelInteractOject()
  {
    Debug.Log($"{_isInteraction} && {_rb.velocity.magnitude != 0}");
    if (_isInteraction && _rb.velocity.magnitude != 0)
    {
      _isInteraction = false;
    }
  }
}
