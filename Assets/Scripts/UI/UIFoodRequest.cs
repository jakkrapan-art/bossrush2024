using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct FoodRequestData
{
  public Sprite FoodImage { get; private set; }
  public Sprite Material1 {  get; private set; } 
  public Sprite Material2 {  get; private set; } 
  public Sprite Material3 {  get; private set; }

  public FoodRequestData(Sprite foodSprite, Sprite material1Sprite, Sprite material2Sprite, Sprite material3Sprite)
  {
    FoodImage = foodSprite;
    Material1 = material1Sprite;
    Material2 = material2Sprite;
    Material3 = material3Sprite;
  }
}

public class UIFoodRequest : UIDialogBase
{
  [SerializeField]
  private RectTransform _rect;

  [SerializeField]
  private Image _foodImage;
  [SerializeField]
  private Image _materialSlot1;
  [SerializeField]
  private Image _materialSlot2;
  [SerializeField]
  private Image _materialSlot3;

  private float START_MATERIAL_X = -25;
  private float MATERIAL_SLOT_WIDTH = 127.5f;

  private void Update()
  {
    if(Input.GetKeyDown(KeyCode.O))
    {

    }
    else if(Input.GetKeyDown(KeyCode.P)) 
    { 

    }
  }

  public void Setup()
  {
    ShowFoodRequest(new FoodRequestData());
  }

  public void ShowFoodRequest(FoodRequestData data)
  {
    int materialCount = 0;
    if(_foodImage != null)
    {
      if(data.FoodImage != null)
      {
        _foodImage.gameObject.SetActive(true);
        _foodImage.sprite = data.FoodImage;
      }
      else
      {
        _foodImage.gameObject.SetActive(false);
      }
    }

    if (_materialSlot1 != null)
    {
      if(data.Material1 != null) 
      {
        materialCount++;
        _materialSlot1.gameObject.SetActive(true);
        _materialSlot1.sprite = data.Material1;
      }
      else
      {
        _materialSlot1.gameObject.SetActive(false);
      }
    }

    if (_materialSlot2 != null)
    {
      if(data.Material2 != null)
      {
        materialCount++;
        _materialSlot2.gameObject.SetActive(true);
        _materialSlot2.sprite = data.Material2;
      }
      else
      {
        _materialSlot2.gameObject.SetActive(false);
      }
    }

    if (_materialSlot3 != null)
    {
      if(data.Material3 != null)
      {
        materialCount++;
        _materialSlot3.gameObject.SetActive(true);
        _materialSlot3.sprite = data.Material3;
      }
      else
      {
        _materialSlot3.gameObject.SetActive(false);
      }
    }

    StartCoroutine(SlowExpandShowMaterial(materialCount));
  }

  public void HideMaterialSlow()
  {
    StartCoroutine(SlowHideMaterialSlot());
  }
  
  private IEnumerator SlowExpandShowMaterial(int count)
  {
    if (_rect == null) yield break;
    float x = START_MATERIAL_X;

    SetBackgroundWidth(208);
    float target = _rect.rect.width + (MATERIAL_SLOT_WIDTH * count);
    yield return SlowSetRectWidth(target);
  }

  private IEnumerator SlowHideMaterialSlot()
  {
    if (_rect == null) yield break;
    float targetX = 208;

    yield return SlowSetRectWidth(targetX);
    ShowFoodRequest(new FoodRequestData());
  }

  private IEnumerator SlowSetRectWidth(float target, float time = 0.5f)
  {
    Func<bool> checkPass;
    bool increase = false;
    if(_rect.sizeDelta.x < target)
    {
      increase = true;
      checkPass = () => { return _rect.sizeDelta.x > target; };
    }
    else
    {
      increase = false;
      checkPass = () => { return _rect.sizeDelta.x < target; };
    }

    while (!checkPass.Invoke())
    {
      float xVal;
      if(increase) xVal = _rect.sizeDelta.x + ((target / time) * Time.deltaTime);
      else xVal = _rect.sizeDelta.x - ((target / time) * Time.deltaTime);

      SetBackgroundWidth(xVal);
      yield return new WaitForEndOfFrame();
    }

    SetBackgroundWidth(_rect.sizeDelta.x);
  }

  private void SetBackgroundWidth(float width)
  {
    if (_rect == null) return;

    _rect.sizeDelta = new Vector2(width, _rect.sizeDelta.y);
  }
}
