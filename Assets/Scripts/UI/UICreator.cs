using System;
using System.Collections.Generic;
using UnityEngine;

public class UICreator : MonoBehaviour
{
  private static UICreator _instance = null;
  private Dictionary<string, GameObject> _dialogObjects = new Dictionary<string, GameObject>();

  private static void CreateInstance()
  {
    if (_instance) throw new System.Exception("Cannot create DialogBuilder more than one.");
    _instance = new GameObject("DialogBuilder").AddComponent<UICreator>();
  }

  public static UICreator GetInstance()
  {
    if(!_instance)
    {
      CreateInstance();
    }

    return _instance;
  }

  private void CreateUI<T>(string path, Action<T> onCreateSuccess) where T : MonoBehaviour
  {
    if (_dialogObjects.ContainsKey(path))
    {
      GameObject dialogObject = _dialogObjects[path];
      if(dialogObject)
      {
        dialogObject.SetActive(true);
        onCreateSuccess(dialogObject as T);
      }
    }
    else
    {
      ObjectPool pool = ObjectPool.GetInstance();
      var obj = pool.Get<T>(path);
      if(obj && obj is MonoBehaviour mono)
      {
        _dialogObjects.Add(path, mono.gameObject);
      }
      onCreateSuccess?.Invoke(obj);
    }
  }

  public void OpenSeedExchangeDialog(List<UISeedExchangeWindow.SlotParam> normalExchangeSlotParams, List<UISeedExchangeWindow.SlotParam> exclusiveExchangeSlotParams, Action<string> onChoose)
  {
    CreateUI<UISeedExchangeWindow>("UISeedExchange", (ui) =>
    {
      if (!ui) return;
      ui.Setup(normalExchangeSlotParams, exclusiveExchangeSlotParams, onChoose);
    });
  }

  public void OpenPauseMenuDialog()
  {

  }

  public void CreateBossRageBar(Action<UIBar> callback)
  {
    CreateUI("BossHpBar", callback);
  }

  public void CreateFoodRequestUI(Action<UIFoodRequest> callback)
  {
    CreateUI("UI/UIFoodRequest", callback);
  }

  public void CreateGameEndDialog(Action<UIGameEnd> callback)
  {
    CreateUI("UI/GameEndDialog", callback);
  }
}
