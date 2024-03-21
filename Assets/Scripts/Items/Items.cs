using System;
using UnityEngine;

public class Items : InteractObject, IPoolingObject
{
  
  public float _timeToDestroy = 60;
  [SerializeField]private ItemData _ItemData;
  private Action _onPickedAction = null;
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

  public Sprite GetIconItem() {  return _ItemData.iconItem; }
  public ItemData GetItemData() {  return _ItemData; }

  protected override void Awake()
  {
    base.Awake();
  }

  public virtual void Kept(GameObject objectHand)
  {
    transform.SetParent(objectHand.transform);
    transform.localPosition = Vector2.zero;
    Destroy(_rb);
    _coll2d.enabled = false;
    _onPickedAction?.Invoke();
    StopCountTimeForDestroy();
  }

  public void Drop(Vector2 position)
  {
    ClearParent();
    StartCountTimeForDestroy(Const.ITEM_LIFETIME);
    transform.position = position;

    _onDroppedAction?.Invoke();
  }

  public void Throw(Vector2 startPoint, Vector2 directionPlayer, float force)
  {
    ClearParent();

    transform.position = startPoint;
    var projectile = gameObject.AddComponent<Projectile>();
    projectile.Setup(this, force, directionPlayer, null) ;
  }

  public void ReturnToPool()
  {
    ObjectPool.ReturnObjectToPool(this);
  }

  private void ClearParent()
  {
    transform.SetParent(null);
    if (_rb == null)
    {
      _rb = gameObject.AddComponent<Rigidbody2D>();
      _rb.gravityScale = 0f;
    }
    if(_coll2d)
    {
      _coll2d.enabled = true;
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
}
