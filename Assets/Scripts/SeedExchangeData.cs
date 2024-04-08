using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeedExchangeData
{
  [field: SerializeField] public string seedName { get; private set; }
  [field: SerializeField] public Sprite shopIcon { get; private set; }
  [field: SerializeField] public Seed seedObj { get; private set; }
}
