using System;
using UnityEngine;

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
  [SerializeField] protected Item _ItemsOutput;

  protected virtual void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    if(_interactColl == null) _interactColl = GetComponent<Collider2D>();
  }

  public virtual InteractResultData Interact(Item interactingItem)
  {
    Item returnItem = null;
    if(_ItemsOutput != null)
    {
      returnItem = ObjectPool.GetInstance().Get<Item>(_ItemsOutput.name);
    }

    return new InteractResultData { waitTime = _timeToInteract, returnItem = returnItem };
  }
}
