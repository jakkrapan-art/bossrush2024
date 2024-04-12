using System;
using UnityEngine;
using UnityEngine.UI;

public class UISeedExchangeSlot : MonoBehaviour
{
  [SerializeField]
  private Image _iconImage = default;
  [SerializeField]
  private Text _seedName = default;
  private Button _button = default;

  public void Setup(Sprite icon, string name, Action onChoose)
  {
    if(_iconImage) _iconImage.sprite = icon;
    if(_seedName) _seedName.text = name;

    _button = GetComponent<Button>();
    if (_button) _button.onClick.AddListener(() => { onChoose?.Invoke(); });
  }
}
