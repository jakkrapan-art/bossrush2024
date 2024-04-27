using System;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogBase : MonoBehaviour, IPoolingObject
{
  [SerializeField]
  protected Button _closeBtn;
  private Action _onCloseAction;

  private void Start()
  {
    if (_closeBtn) _closeBtn.onClick.AddListener(() =>
    {
      Close();
    });
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
    ObjectPool.ReturnObjectToPool(this);
  }

  public virtual void ResetPoolingObject() {}
}
