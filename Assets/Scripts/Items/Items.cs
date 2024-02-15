using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Items : InteractOdject
{
  
  public float TimeToDestroy = 60;
  private bool _isHolding;
  private float _timeCountdownToDestoy;
  [SerializeField]private ItemData _ItemData;
  [SerializeField]private SpriteRenderer _itemSprite;
  private int _defaultSortingOrder = 0;
  public Sprite GetIconItem() {  return _ItemData.iconItem; }
  public ItemData GetItemData() {  return _ItemData; }

  protected override void Awake()
  {
    base.Awake();

    _itemSprite = GetComponent<SpriteRenderer>();
    if(_itemSprite) _defaultSortingOrder = _itemSprite.sortingOrder;
  }

  public virtual void Kept(GameObject objectHand)
  {
    _isHolding = true;
    transform.SetParent(objectHand.transform);
    transform.localPosition = Vector2.zero;
    Destroy(_rb);
    _coll2d.enabled = false;
    if (_itemSprite) _itemSprite.sortingOrder += 1; 
  }

  

  public void Drop(Vector2 position)
  {
    _isHolding = false;
    transform.SetParent(null);
    transform.position = position;
    if (_rb == null)
    {
      _rb = gameObject.AddComponent<Rigidbody2D>();
      _rb.gravityScale = 0f;
    }
    _coll2d.enabled = true;
    if (_itemSprite) _itemSprite.sortingOrder -= 1;
  }
  public void Throw(Vector2 directionPlayer)
  {
    _isHolding = false;
    transform.SetParent(null);
    if (_rb == null)
    {
      _rb = gameObject.AddComponent<Rigidbody2D>();
      _rb.gravityScale = 0f;
    }
    _rb.velocity = directionPlayer;
    _coll2d.enabled = true;
    if (_itemSprite) _itemSprite.sortingOrder = _defaultSortingOrder;
  }

 

  // Update is called once per frame
  protected virtual void Update()
  {
    if (!_isHolding)
    {
      _timeCountdownToDestoy += Time.deltaTime;
      if (_timeCountdownToDestoy > TimeToDestroy)
      {
        Destroy(this.gameObject);
      }
    }
    else
    {
      _timeCountdownToDestoy = 0;
    }
    if(_rb != null)
    if (_rb.velocity != Vector2.zero)
    {
      _rb.velocity -= _rb.velocity * Time.fixedDeltaTime;
    }
  }
}
