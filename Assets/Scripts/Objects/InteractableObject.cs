using System;
using UnityEngine;

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
  protected float _lastInteract = 0;
  [SerializeField] protected Item _ItemsOutput;

  protected virtual void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    if(_interactColl == null) _interactColl = GetComponent<Collider2D>();
    if(!gameObject.TryGetComponent(out DynamicSortingOrder _)) gameObject.AddComponent<DynamicSortingOrder>();
  }

  public virtual InteractResultData Interact(Item interactingItem)
  {
    if (_lastInteract != 0 && Time.time < _lastInteract + _cooldown) return new InteractResultData();

    Item returnItem = null;
    if(_ItemsOutput != null)
    {
      returnItem = ObjectPool.GetInstance().Get<Item>(_ItemsOutput.name);
    }

    _lastInteract = Time.time;
    return new InteractResultData { waitTime = _timeToInteract, returnItem = returnItem };
  }
}
