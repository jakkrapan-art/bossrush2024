using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DynamicSortingOrder))]
public class InteractableObject : MonoBehaviour
{
  public struct InteractResultData
  {
    public Item returnItem;
    public float waitTime;
    public bool clearHand;
    public Func<WaitResultData> waitResult;
  }

  public struct WaitResultData
  {
    public bool finish;
    public Item resultItem;
  }

  protected Rigidbody2D _rb;
  [SerializeField]
  protected Collider2D _interactColl;

  [SerializeField] protected float _timeToInteract = 0;
  [SerializeField] protected float _cooldown = 0;
  [SerializeField] protected Image _cooldownUI = null;
  protected float _lastInteract = 0;
  [SerializeField] protected Item _ItemsOutput;

  protected virtual void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    if(_interactColl == null) _interactColl = GetComponent<Collider2D>();
    if(!gameObject.TryGetComponent(out DynamicSortingOrder _)) gameObject.AddComponent<DynamicSortingOrder>();
  }

  private void Start()
  {
    SetActiveCooldownUI(false);
  }

  private void Update()
  {
    if(isCooldown)
    {
      UpdateCooldownUI();
    }
    else
    {
      SetActiveCooldownUI(false);
    }
  }

  public virtual InteractResultData Interact(Item interactingItem)
  {
    if (isCooldown)
    {
      return new InteractResultData();
    }

    Item returnItem = null;
    if(_ItemsOutput != null)
    {
      var prefix = Const.GetObjectPrefix(_ItemsOutput.GetType());
      returnItem = ObjectPool.GetInstance().Get<Item>(prefix + _ItemsOutput.name);
    }

    _lastInteract = Time.time;
    return new InteractResultData { waitTime = _timeToInteract, returnItem = returnItem };
  }

  private bool isCooldown => _lastInteract != 0 && Time.time < _lastInteract + _cooldown;

  private void UpdateCooldownUI()
  {
    if (_cooldownUI == null) return;

    if (!_cooldownUI.gameObject.activeSelf) SetActiveCooldownUI(true);
    var fillAmount = Mathf.Clamp((Time.time - _lastInteract) / _cooldown, 0, 1);
    _cooldownUI.fillAmount = fillAmount;
  }

  private void SetActiveCooldownUI(bool active)
  {
    if (_cooldownUI != null) _cooldownUI.gameObject.SetActive(active);
  }
}
