using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossParticle : MonoBehaviour
{
  private Coroutine _hideDelayCoroutine = null;

  private void Start()
  {
    Hide();
  }

  public void Show(float second)
  {
    gameObject.SetActive(true);
    if(_hideDelayCoroutine != null)
    {
      StopCoroutine(_hideDelayCoroutine);
      _hideDelayCoroutine = null;
    }

    _hideDelayCoroutine = StartCoroutine(DelayHide(second));
  }

  private IEnumerator DelayHide(float second)
  {
    yield return new WaitForSeconds(second);
    Hide();
  }

  public void Hide()
  {
    gameObject.SetActive(false);
  }
}
