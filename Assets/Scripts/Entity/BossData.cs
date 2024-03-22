using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Data/Boss")]
public class BossData : EntityData
{
  [field: SerializeField] public int MaxPlant{ get; private set; } = 7;
  [field: SerializeField] public Product[] PossibleRequestFoods { get; private set; }
}
