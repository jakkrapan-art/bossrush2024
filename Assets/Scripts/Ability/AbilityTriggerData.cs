using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AbilityTriggerData
{
  [field: SerializeField] public Ability ability { get; private set; }
  [field: SerializeField] public float triggerChance { get; private set; } = 0.2f;
  [field: SerializeField] public int rageMin { get; private set; } = 0;
  [field: SerializeField] public int rageMax { get; private set; } = 100;
}
