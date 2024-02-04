using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISeedExchangeWindow : MonoBehaviour
{
  [SerializeField]
  private Button _closeBtn;
  [SerializeField]
  private List<UISeedExchangeSlot> _normalSlots;
  [SerializeField]
  private List<UISeedExchangeSlot> _exclusiveSlots;

  public struct SlotParam
  {
    public Sprite iconImage;
    public string seedName;
    public Action onChoose;
  }

  public void Setup(List<SlotParam> nmSlotsParam, List<SlotParam> exSlotsParam)
  {
    void setupSlot(List<UISeedExchangeSlot> slotList, List<SlotParam> slotParamList)
    {
      for (int i = 0; i < nmSlotsParam.Count; i++)
      {
        if (i > slotList.Count - 1) break;
        else if (!slotList[i]) continue;

        var slot = slotList[i];
        var param = slotParamList[i];

        slot.Setup(param.iconImage, param.seedName, param.onChoose);
      }
    }

    setupSlot(_normalSlots, nmSlotsParam);
    setupSlot(_exclusiveSlots, exSlotsParam);

    if (_closeBtn) _closeBtn.onClick.AddListener(() => 
    {
      Hide();
    });
  }

  public void Show()
  {
    gameObject.SetActive(true);
  }

  public void Hide()
  {
    gameObject.SetActive(false);
  }
}
