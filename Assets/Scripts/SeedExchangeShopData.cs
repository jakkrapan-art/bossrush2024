using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SeedExchangeShopData", menuName = "Data/SeedExchangeShop")]
public class SeedExchangeShopData : ScriptableObject
{
  public List<SeedExchangeData> normalSeedData;
  public List<SeedExchangeData> exclusiveSeedData;
}
