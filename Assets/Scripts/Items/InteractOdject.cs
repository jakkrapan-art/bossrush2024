using Jakkrapan.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractOdject : MonoBehaviour
{
  protected Rigidbody2D _rb;
  protected Collider2D _coll2d;

  [SerializeField] protected RecipeFoods _listItemCanInteract;
  [SerializeField] protected float _timeToInteract = 0;
  [SerializeField] protected Items _ItemsOutput;
  private Player _interactor = null;
  // Start is called before the first frame update

  protected virtual void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    _coll2d = GetComponent<Collider2D>();
  }

  public void Interact(Player interactor)
  {
    _interactor = interactor;
  }

  public virtual bool CanInteract(Items itemToInteract)
  {
    if (_listItemCanInteract == null) return true;

    foreach (ItemData item in _listItemCanInteract.ItemsInput)
    {
      if (item == itemToInteract)
      {
        return true;
      }
    }
    return false;
  }

  public float GetTimeToInteract() { return _timeToInteract; }

  public virtual void InteractResult()
  {
    if(_interactor && _ItemsOutput)
    {
      var item = ObjectPool.GetInstance().Get<Items>(_ItemsOutput.name);
      if(item)
      {
        _interactor.PickItem(item);
      }
    }

    _interactor = null;
  }
}
