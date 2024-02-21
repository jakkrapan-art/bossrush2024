using System;
using UnityEngine;

[RequireComponent(typeof(PoolingObject))]
public class UIDialogBase : MonoBehaviour
{
  private PoolingObject _poolingObject;
  private Action _onCloseAction;

  protected virtual void Awake()
  {
    _poolingObject = GetComponent<PoolingObject>();
  }

  public void AddCloseListener(Action action)
  {
    _onCloseAction += action;
  }

  public void ReplaceCloseListener(Action action)
  {
    _onCloseAction = action;
  }

  public virtual void Close()
  {
    _onCloseAction?.Invoke();

    if (_poolingObject) _poolingObject.ReturnToPool();
    else Destroy(gameObject);
  }
}
