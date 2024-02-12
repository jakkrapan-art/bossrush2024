using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogBuilder : MonoBehaviour
{
  private static DialogBuilder _instance = null;

  private static void CreateInstance()
  {
    if (_instance) throw new System.Exception("Cannot create DialogBuilder more than one.");
    _instance = new GameObject("DialogBuilder").AddComponent<DialogBuilder>();
  }

  public static DialogBuilder GetInstance()
  {
    if(!_instance)
    {
      CreateInstance();
    }

    return _instance;
  }

  private void CreateUI<T>(string path, Action<T> onCreateSuccess) where T : MonoBehaviour
  {
    ObjectPool pool = ObjectPool.GetInstance();
    var obj = pool.Get<T>(path);
    onCreateSuccess?.Invoke(obj);
  }

  public void OpenSeedExchangeDialog(List<UISeedExchangeWindow.SlotParam> normalExchangeSlotParams, List<UISeedExchangeWindow.SlotParam> exclusiveExchangeSlotParams)
  {
    CreateUI<UISeedExchangeWindow>("UISeedExchange", (ui) =>
    {
      if (!ui) return;
      ui.Setup(normalExchangeSlotParams, exclusiveExchangeSlotParams);
    });
  }

  public void OpenPauseMenuDialog()
  {

  }
}
