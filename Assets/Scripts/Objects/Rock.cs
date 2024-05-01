using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour, IPoolingObject
{
  [SerializeField]
  private Animator _animator;
  [SerializeField]
  private Collider2D _collider2d;

  private float _lifetime = 0f;
  private float _spawnTime = 0f;
  private bool _startCountTime = false;
  private bool _unbreakable = false;
  private bool _flyIn = false;

  private void Update()
  {
    if (_unbreakable || Time.frameCount % 10 != 0 || !_startCountTime) return;

    if(Time.time >= _spawnTime + _lifetime)
    {
      ObjectPool.ReturnObjectToPool(this);
    }
  }

  private void StartCountTime()
  {
    _startCountTime = true;
    _spawnTime = Time.time;
    _animator.SetBool("active", !_unbreakable);
  }

  public void ResetPoolingObject() 
  {
    _startCountTime = false;
    _animator.SetBool("active", false);
    _flyIn = false;
  }

  public void Setup(float lifetime, bool unbreakable, float animationSpeed)
  {
    _unbreakable = unbreakable;
    _lifetime = lifetime;

    if(_animator != null )
    {
      _animator.speed = animationSpeed;
    }
  }

  public void Spawn()
  {
    if (_flyIn) return;
    StartCoroutine(FlyIn(StartCountTime));
  }

  private IEnumerator FlyIn(Action onCompleted)
  {
    _flyIn = true;
    bool collEnableTemp = _collider2d == null ? false : _collider2d.enabled;

    SetEnableCollider(false);
    Vector3 endPos = transform.position;
    Vector3 startPos = endPos + new Vector3(0, 10);
    float elapsedTime = 0f;
    float yDistance = startPos.y - endPos.y;
    float duration = 0.35f;
    float startTime = Time.time;

    transform.position = startPos;
    while(Time.time < startTime + duration)
    {
      Vector3 currPos = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
      transform.position = currPos;
      elapsedTime += Time.deltaTime;
      yield return new WaitForEndOfFrame();
    }

    transform.position = endPos;
    SetEnableCollider(collEnableTemp);
    onCompleted?.Invoke();
  }

  private void SetEnableCollider(bool enable)
  {
    if (_collider2d != null) { _collider2d.enabled = enable; }
  }
}
