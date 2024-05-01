using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISeedExchangeWindow : UIDialogBase
{
  [SerializeField]
  private List<UISeedExchangeSlot> _normalSlots;
  [SerializeField]
  private List<UISeedExchangeSlot> _exclusiveSlots;

  public struct SlotParam
  {
    public Sprite iconImage;
    public string seedName;
  }

  public void Setup(List<SlotParam> nmSlotsParam, List<SlotParam> exSlotsParam, Action<string> onChoose, Action onClose)
  {
    AddCloseListener(onClose);

    void setupSlot(List<UISeedExchangeSlot> slotList, List<SlotParam> slotParamList)
    {
      if (slotList != null && slotParamList != null)
      {
        for (int i = 0; i < nmSlotsParam.Count; i++)
        {
          if (i > slotList.Count - 1) break;
          else if (!slotList[i]) continue;

          var slot = slotList[i];
          var param = slotParamList[i];

          slot.Setup(param.iconImage, param.seedName, ()=> 
          {
            onChoose?.Invoke(param.seedName);
            Close();
          });
        }
      }
    }

    setupSlot(_normalSlots, nmSlotsParam);
    setupSlot(_exclusiveSlots, exSlotsParam);

    Canvas.ForceUpdateCanvases();
  }
}
