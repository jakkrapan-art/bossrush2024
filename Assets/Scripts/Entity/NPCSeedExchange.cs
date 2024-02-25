using System.Collections.Generic;
using UnityEngine;

public class NPCSeedExchange : InteractObject
{
  [SerializeField] private SeedExchangeShopData _shopData;

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
      if (seed)
      {
        seed.transform.SetParent(null);
        _ItemsOutput = seed;
        InteractResult();
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
        onChoose = () => onChoose(d.seedName.Replace(" ", ""))
    });
    }
  }

  public override void InteractResult()
  {
    if(_ItemsOutput)
    {
      base.InteractResult();
      _ItemsOutput = null;
    }
    else
    {
      DialogBuilder.GetInstance().OpenSeedExchangeDialog(_normalShopSlotParams, _exclusiveShopSlotParams);
    }
  }
}
