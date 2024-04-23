using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossParticle : MonoBehaviour
{
  private Coroutine _hideDelayCoroutine = null;

  public void Show(float second, Action callback = null)
  {
    gameObject.SetActive(true);
    if (second < 0) return;
    if(_hideDelayCoroutine != null)
    {
      StopCoroutine(_hideDelayCoroutine);
      _hideDelayCoroutine = null;
    }

    _hideDelayCoroutine = StartCoroutine(DelayHide(second, callback));
  }

  private IEnumerator DelayHide(float second, Action callback)
  {
    yield return new WaitForSeconds(second);
    Hide(callback);
  }

  public void Hide(Action callback = null)
  {
    callback?.Invoke();
    gameObject.SetActive(false);
  }
}
