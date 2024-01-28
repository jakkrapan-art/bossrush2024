using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractOdject : MonoBehaviour
{
  protected Rigidbody2D _rb;
  protected BoxCollider2D _coll2d;

  [SerializeField] protected RecipeFoods _listItemCanInteract;
  [SerializeField] protected float _timeToInteract = 0;
  [SerializeField] protected Items _ItemsOutput;
  // Start is called before the first frame update

  private void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    _coll2d = GetComponent<BoxCollider2D>();
  }

  public virtual bool CanInteract(Items itemToInteract)
  {
    if (_listItemCanInteract == null) return true;

    foreach (Items item in _listItemCanInteract.ItemsInput)
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
    Debug.Log("get new item");
  }
}