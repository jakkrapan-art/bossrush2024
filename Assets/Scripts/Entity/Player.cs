using UnityEngine;

public class Player : Entity
{
  [SerializeField]
  private GameObject _handHoldingItem;
  private Items _itemHolding;
  private InteractOdject _itemFinded;
  private InteractOdject _itemInteraction;

  [SerializeField]
  private float _forceToThrow = 50;
  private bool _isInteraction;
  private float _currentTimer;
  private float _timeToInteraction;

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
    else if (_itemFinded.GetComponent<Items>())
    {
      _itemHolding = _itemFinded.GetComponent<Items>();
      _itemFinded.GetComponent<Items>().Kept(_handHoldingItem);
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
    if (_itemFinded && !_isInteraction)
    {
      if (_itemFinded.CanInteract(_itemHolding))
      {
        _isInteraction = true;
        _timeToInteraction = _itemFinded.GetTimeToInteract();
        _itemInteraction = _itemFinded;
        _currentTimer = 0;
        Debug.Log("StartInteractOject");

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
      Debug.Log("InteractFinish");
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
    if (collision.GetComponent<InteractOdject>())
    {
      _itemFinded = collision.GetComponent<InteractOdject>();
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
