using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoutSkill : BossSkill
{
  public ShoutSkill(Transform[] possibleTarget,bool ready, float cooldown = 15) : base(ready, cooldown)
  {
  }

  public override bool Use()
  {


    return base.Use();
  }
}
