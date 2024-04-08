using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill
{
  private bool _ready = true;
  private float _cooldown = 15;
  private float _lastUse = 0;

  public BossSkill(bool ready, float cooldown = 15) 
  {
    if(!ready) _lastUse = Time.time;
    _ready = ready;
    _cooldown = cooldown;
  }
  public bool IsReady() 
  {
    if(!_ready && (_lastUse + _cooldown >= Time.time)) _ready = true;
    return _ready;
  }

  public virtual bool Use()
  {
    _lastUse = Time.time;
    _ready = false;
    return _ready;
  }
}
