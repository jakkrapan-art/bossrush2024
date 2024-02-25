using UnityEngine;

public class Player : Entity
{
  [SerializeField]
  private GameObject _handHoldingItem;
  [SerializeField]
  private Transform _dropItemPosition;
  [SerializeField]
  private Transform _throwItemPoint;
  private Items _holdingItem;
  private InteractObject _itemFinded;
  private InteractObject _interactingObject;

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
    if (!item) return;

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
      _holdingItem.Throw(_throwItemPoint.position, directionPlayer * _forceToThrow);
      _holdingItem = null;
    }
  }
  public void StartInteractObject()
  {
    if (!_objDetector) return;
    var interactTarget = _objDetector.GetInteractObject();
    if (interactTarget && !_isInteraction && interactTarget.CanInteract(_holdingItem))
    {
      interactTarget.Interact(this);
      _isInteraction = true;
      _timeToInteraction = interactTarget.GetTimeToInteract();
      _interactingObject = interactTarget;
      _currentTimer = 0;
      //SetEnableMove(false);
    }
  }
  public void InteractFinish()
  {
    if (_interactingObject && _isInteraction)
    {
      _isInteraction = false;
      _interactingObject.InteractResult();
      _interactingObject = null;
    }
    SetEnableMove(true);
  }
}
