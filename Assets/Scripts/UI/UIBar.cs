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

  public float GetBarValue() => _bar.value;
  public void UpdateValue(float updateValue)
  {
    float value = _bar.value + updateValue;

    if (_bar) _bar.value = value;
    if (_barPercentValue)
    {
      _barPercentValue.text = Mathf.RoundToInt(value * 100) + "%";
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

  public void SetBarFillColor(Color color)
  {
    if (_barFill) _barFill.color = color;
  }

  public Color GetBarFillColor()
  {
    if (_barFill) return _barFill.color;
    return Color.black;
  }
}
