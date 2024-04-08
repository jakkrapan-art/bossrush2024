using System;
using UnityEngine;
using UnityEngine.Pool;

public class UIDialogBase : MonoBehaviour, IPoolingObject
{
  private Action _onCloseAction;

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
    ObjectPool.ReturnObjectToPool(this);
  }

  public virtual void ResetPoolingObject() {}
}
