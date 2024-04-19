using System;
using UnityEngine;

public class Item : InteractableObject, IPoolingObject
{
  
  public float _timeToDestroy = 60;
  [SerializeField]private ItemData _ItemData;
  private Action _onPickedAction = null;
  private DynamicSortingOrder _sortingOrder;

  public void AddOnPickedListener(Action action)
  {
    _onPickedAction += action;
  }
  public void RemoveOnPickedListener(Action action)
  {
    _onPickedAction -= action;
  }

  private Action _onDroppedAction = null;
  public void AddOnDroppedAction(Action action)
  {
    _onDroppedAction += action;
  }

  public void RemoveOnDroppedAction(Action action)
  {
    _onDroppedAction -= action;
  }

  private bool _countingTime = false;

  public Sprite GetIconSprite() {  return _ItemData.icon; }
  public ItemData GetItemData() {  return _ItemData; }

  protected override void Awake()
  {
    base.Awake();
    _sortingOrder = GetComponent<DynamicSortingOrder>();
  }

  public virtual void Kept(GameObject objectHand)
  {
    transform.SetParent(objectHand.transform);
    transform.localPosition = Vector2.zero;
    Destroy(_rb);
    _interactColl.enabled = false;
    _onPickedAction?.Invoke();
    StopCountTimeForDestroy();
  }

  public void Drop(Vector2 position)
  {
    ClearParent();
    StartCountTimeForDestroy(Const.ITEM_LIFETIME);
    transform.position = position;

    _onDroppedAction?.Invoke();
    if(_sortingOrder != null)
    {
      _sortingOrder.AdjustSortingOrder();
    }
  }

  public void Throw(Vector2 startPoint, Vector2 directionPlayer, float force)
  {
    ClearParent();

    transform.position = startPoint;
    Projectile projectile;
    if(!TryGetComponent(out projectile))
    {
      projectile = gameObject.AddComponent<Projectile>();
    }

    if (_sortingOrder != null) _sortingOrder.SetAlwaysUpdate(true);
    projectile.Setup(this, force, directionPlayer, null, () => { if (_sortingOrder != null) _sortingOrder.SetAlwaysUpdate(false); });
  }

  public void ReturnToPool()
  {
    if (_interactColl) _interactColl.enabled = true;
    ObjectPool.ReturnObjectToPool(this);
  }

  private void ClearParent()
  {
    transform.SetParent(null);
    if (_rb == null)
    {
      _rb = AddRigidBody();
    }
    if(_interactColl)
    {
      _interactColl.enabled = true;
    }
  }

  private void StopCountTimeForDestroy()
  {
    _countingTime = false;
  }

  private void StartCountTimeForDestroy(float time)
  {
    _countingTime = true;
    _timeToDestroy = Time.time + time;
  }

  // Update is called once per frame
  private void Update()
  {
    if (!_countingTime) return;

    if (Time.time >= _timeToDestroy)
    {
      ReturnToPool();
    }
  }

  public virtual void ResetPoolingObject() {}

  private Rigidbody2D AddRigidBody()
  {
    var rb = gameObject.AddComponent<Rigidbody2D>();
    rb.gravityScale = 0f;
    return rb;
  }
}