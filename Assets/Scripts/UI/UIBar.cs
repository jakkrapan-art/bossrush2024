using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
  [SerializeField]
  private Slider _bar = null;
  [SerializeField]
  private Image _barFill = null;
  [SerializeField]
  private Text _barPercentValue = null;
  private Action<float> _onUpdateAction = null;
  public void UpdateValue(float value)
  {
    if(_bar) _bar.value = value;
    if(_barPercentValue)
    {
      _barPercentValue.text = Mathf.RoundToInt(value * 100) + "%";
      var rect = _barPercentValue.GetComponent<RectTransform>();
      if(rect)
      {
        rect.pivot = new Vector2(value, rect.pivot.x);
      }

      _onUpdateAction?.Invoke(value);
    }
  }

  public void SubscribeOnUpdateAction(Action<float> action)
  {
    _onUpdateAction += action;
  }

  public void UnsubscribeOnUpdateAction(Action<float> action)
  {
    _onUpdateAction -= action;
  }

  public void SetBarColor(Color color)
  {
    if(_barFill) _barFill.color = color;
  }
}
