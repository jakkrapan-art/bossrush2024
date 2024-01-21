using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Items : MonoBehaviour
{
  protected Rigidbody2D _rb;
  public float TimeToDestroy = 60;
  private bool isHolding;
  private float timeCountdownToDestoy;

  private void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
  }

  void Start()
  {

  }

  public void Kept(GameObject objectHand)
  {
    isHolding = true;
    transform.parent = objectHand.transform;
    transform.localPosition = Vector2.zero;
    
  }

  public void Droped()
  {
    isHolding = false;
    transform.parent = null;
  }
  public void Throw(Vector2 directionPlayer)
  {
    isHolding = false;
    transform.parent = null;
    _rb.velocity = directionPlayer;
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
    if (_rb.velocity == Vector2.zero)
    {
      _rb.velocity -= _rb.velocity * Time.fixedDeltaTime;
    }
  }
}
