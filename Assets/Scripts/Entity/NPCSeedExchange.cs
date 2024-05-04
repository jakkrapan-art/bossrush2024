using System.Collections.Generic;
using UnityEngine;

public class NPCSeedExchange : InteractableObject
{
  [SerializeField] private SeedExchangeShopData _shopData;

  private List<UISeedExchangeWindow.SlotParam> _normalShopSlotParams;
  private List<UISeedExchangeWindow.SlotParam> _exclusiveShopSlotParams;

  private Seed _resultSeed;
  private bool _closed = false;

  private void Start()
  {
    Setup();
  }

  private void Setup()
  {
    if (!_shopData) return;
    
    _normalShopSlotParams = new List<UISeedExchangeWindow.SlotParam>();
    foreach (var d in _shopData.normalSeedData)
    {
      _normalShopSlotParams.Add(new UISeedExchangeWindow.SlotParam
      {
        iconImage = d.shopIcon,
        seedName = d.seedName,
      });
    }

    _exclusiveShopSlotParams = new List<UISeedExchangeWindow.SlotParam>();
    foreach (var d in _shopData.exclusiveSeedData)
    {
      _exclusiveShopSlotParams.Add(new UISeedExchangeWindow.SlotParam
      {
        iconImage = d.shopIcon,
        seedName = d.seedName,
    });
    }
  }

  public override InteractResultData Interact(Item interactingItem)
  {
    _closed = false; ;
    UICreator.GetInstance().OpenSeedExchangeDialog(_normalShopSlotParams, _exclusiveShopSlotParams, (plantName) => 
    {
      var seed = ObjectPool.GetInstance().Get<Seed>(plantName.Replace(" ", "") + "_seed");
      if(seed != null) _resultSeed = seed;
    }, () => 
    {
      _closed = true;
    });

    return new InteractResultData { waitResult = waitForChooseSeed };
  }

  private WaitResultData waitForChooseSeed()
  {
    if (_closed && _resultSeed == null) return new WaitResultData { finish = true };

    if(_resultSeed == null) return new WaitResultData { finish = false };

    Seed resultSeed = _resultSeed;
    _resultSeed = null;
    return new WaitResultData { finish = true, resultItem = resultSeed };
  }
}
