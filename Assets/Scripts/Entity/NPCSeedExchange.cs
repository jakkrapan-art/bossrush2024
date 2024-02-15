using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jakkrapan.ObjectPool;

public class NPCSeedExchange : InteractOdject
{
  [SerializeField] private SeedExchangeShopData _shopData;

  [SerializeField] private Transform _dropProductPoint;

  private List<UISeedExchangeWindow.SlotParam> _normalShopSlotParams;
  private List<UISeedExchangeWindow.SlotParam> _exclusiveShopSlotParams;

  private void Start()
  {
    Setup();
  }

  private void Setup()
  {
    if (!_shopData) return;
    void onChoose(string name)
    {
      var objPool = ObjectPool.GetInstance();
      var seed = objPool.Get<Seed>(name + "_seed");
      seed.transform.SetParent(null);
      if(_dropProductPoint)
      {
        seed.transform.position = _dropProductPoint.position;
      }
    }
    
    _normalShopSlotParams = new List<UISeedExchangeWindow.SlotParam>();
    foreach (var d in _shopData.normalSeedData)
    {
      _normalShopSlotParams.Add(new UISeedExchangeWindow.SlotParam
      {
        iconImage = d.shopIcon,
        seedName = d.seedName,
        onChoose = () => onChoose(d.seedName.Replace(" ", "")),
      });
    }

    _exclusiveShopSlotParams = new List<UISeedExchangeWindow.SlotParam>();
    foreach (var d in _shopData.exclusiveSeedData)
    {
      _exclusiveShopSlotParams.Add(new UISeedExchangeWindow.SlotParam
      {
        iconImage = d.shopIcon,
        seedName = d.seedName,
        onChoose = () => onChoose(d.seedName.Replace(" ", "")),
      });
    }
  }

  public override void InteractResult(Player player = null)
  {
    base.InteractResult(null);
    DialogBuilder.GetInstance().OpenSeedExchangeDialog(_normalShopSlotParams, _exclusiveShopSlotParams);
  }
}
