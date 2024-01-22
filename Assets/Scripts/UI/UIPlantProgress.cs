using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlantProgress : MonoBehaviour
{
  [SerializeField]
  private Image _progressFill = default;

  public void SetActive(bool active)
  {
    gameObject.SetActive(active);
  }

  public void UpdateProgressValue(float value)
  {
    if(_progressFill) _progressFill.fillAmount = value;
  }
}
