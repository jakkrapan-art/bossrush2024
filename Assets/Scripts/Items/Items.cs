using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Items : MonoBehaviour
{
  protected Rigidbody2D _rb;
  protected BoxCollider2D _coll2d;
  public float TimeToDestroy = 60;
  private bool isHolding;
  private float timeCountdownToDestoy;

  private void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    _coll2d = GetComponent<BoxCollider2D>();
  }

  void Start()
  {

  }

  public void Kept(GameObject objectHand)
  {
    isHolding = true;
    transform.parent = objectHand.transform;
    transform.localPosition = Vector2.zero;
    Destroy(_rb);
    _coll2d.enabled = false;
  }

  public void Droped()
  {
    isHolding = false;
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
    isHolding = false;
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
  void Update()
  {
    if (!isHolding)
    {
      timeCountdownToDestoy += Time.deltaTime;
      if (timeCountdownToDestoy > TimeToDestroy)
      {
        Destroy(this.gameObject);
      }
    }
    else
    {
      timeCountdownToDestoy = 0;
    }
    if(_rb != null)
    if (_rb.velocity != Vector2.zero)
    {
      _rb.velocity -= _rb.velocity * Time.fixedDeltaTime;
    }
  }
}
