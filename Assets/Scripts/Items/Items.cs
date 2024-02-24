using Unity.VisualScripting;
using UnityEngine;

public class Items : InteractOdject
{
  
  public float _timeToDestroy = 60;
  [SerializeField]private ItemData _ItemData;
  [SerializeField]private SpriteRenderer _itemSprite;

  private bool _countingTime = false;

  public Sprite GetIconItem() {  return _ItemData.iconItem; }
  public ItemData GetItemData() {  return _ItemData; }

  protected override void Awake()
  {
    base.Awake();

    _itemSprite = GetComponent<SpriteRenderer>();
  }

  public virtual void Kept(GameObject objectHand)
  {
    transform.SetParent(objectHand.transform);
    transform.localPosition = Vector2.zero;
    Destroy(_rb);
    _coll2d.enabled = false;

    StopCountTimeForDestroy();
  }

  public void Drop(Vector2 position)
  {
    ClearParent();
    StartCountTimeForDestroy(Const.ITEM_LIFETIME);
    transform.position = position;
  }

  public void Throw(Vector2 startPoint, Vector2 directionPlayer)
  {
    ClearParent();

    transform.position = startPoint;
    var projectile = gameObject.AddComponent<Projectile>();
    projectile.Setup(this, 25, directionPlayer, (hit) => { Debug.Log("hit:" + hit); });
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
    /*if (_rb != null)
      if (_rb.velocity != Vector2.zero)
      {
        _rb.velocity -= _rb.velocity * Time.fixedDeltaTime;
      }*/
}
