using System;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
  public struct ResultData
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
  protected Collider2D _coll2d;

  [SerializeField] protected RecipeFoods _listItemCanInteract;
  [SerializeField] protected float _timeToInteract = 0;
  [SerializeField] protected Item _ItemsOutput;

  protected virtual void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    _coll2d = GetComponent<Collider2D>();
  }

  public virtual ResultData Interact(Item interactingItem)
  {
    Item returnItem = null;
    if(_ItemsOutput != null)
    {
      returnItem = ObjectPool.GetInstance().Get<Item>(_ItemsOutput.name);
    }

    return new ResultData { waitTime = _timeToInteract, returnItem = returnItem };
  }
}
