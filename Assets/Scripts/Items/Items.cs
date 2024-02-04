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
  [SerializeField]private SpriteRenderer _ItemSprite;
  public Sprite GetIconItem() {  return _ItemData.iconItem; }
  public ItemData GetItemData() {  return _ItemData; }



  public virtual void Kept(GameObject objectHand)
  {
    _isHolding = true;
    transform.parent = objectHand.transform;
    transform.localPosition = Vector2.zero;
    Destroy(_rb);
    _coll2d.enabled = false;
  }

  

  public void Droped()
  {
    _isHolding = false;
    transform.parent = null;
    if (_rb == null)
    {
      _rb = gameObject.AddComponent<Rigidbody2D>();
      _rb.gravityScale = 0f;
    }
    _coll2d.enabled = true;
  }
  public void Throw(Vector2 directionPlayer)
  {
    _isHolding = false;
    transform.parent = null;
    if (_rb == null)
    {
      _rb = gameObject.AddComponent<Rigidbody2D>();
      _rb.gravityScale = 0f;
    }
    _rb.velocity = directionPlayer;
    _coll2d.enabled = true;
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
