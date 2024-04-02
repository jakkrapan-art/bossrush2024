using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFoodRequestBubble : UIBossParticle
{
  [SerializeField] private Image _foodImage = default;

  public void SetFoodImage(Sprite image)
  {
    if (_foodImage != null) _foodImage.sprite = image; 
  }
}
