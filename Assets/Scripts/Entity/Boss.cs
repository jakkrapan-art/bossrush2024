using System;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class Boss : Entity, IHitableObject
{
  private readonly float MAX_RAGE = 100;
  [SerializeField]
  private float _rage = 50.00f;
  [SerializeField]
  private float _ragePointIncreased = 0.16f;

  private UIBar _bossRageBar = null;
  protected override void Awake()
  {
    base.Awake();
    _stateMachine = new BossStateMachine();
  }

  private void Start()
  {
    DialogBuilder.GetInstance().CreateBossRageBar((bar) => 
    {
      if (!bar) return;

      bar.UpdateValue(getRagePointPercentage());
      bar.SubscribeOnUpdateAction((val) =>
      {
        if (val > 0.75f)
        {
          bar.SetBarFillColor(Color.red);
        }
        else if (val > 0.50f)
        {
          bar.SetBarFillColor(new Color32(255, 144, 0, 255)); //orange
        }
        else if (val > 0.25f)
        {
          bar.SetBarFillColor(Color.yellow);
        }
        else
        {
          bar.SetBarFillColor(Color.green);
        }
      });

      _bossRageBar = bar;
    });
  }

  protected override void Update()
  {
    UpdateRageValue(_ragePointIncreased * Time.deltaTime);
    _stateMachine?.Update();
  }

  private void FixedUpdate()
  {
    _stateMachine?.FixedUpdate();
  }

  private void UpdateRageValue(float updateValue)
  {
    _rage = Mathf.Clamp(_rage + updateValue, 0, MAX_RAGE);
    handleStateByRagePoint();

    if (_bossRageBar)
    {
      _bossRageBar.UpdateValue(getRagePointPercentage());
    }
  }

  private void StartRageIncreaseTimer()
  {

  }

  private void handleStateByRagePoint()
  {
    float ragePercentage = getRagePointPercentage();
    BossStateMachine bossStateMachine = (BossStateMachine)_stateMachine;
    if (ragePercentage >= 0.80f && ragePercentage < 0.99f)
    {
      if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.RageState))
      {
        bossStateMachine.ChangeState(bossStateMachine.RageState);
      }
    }
    else if (ragePercentage >= 0.99f)
    {
      if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.EnrageState))
      {
        bossStateMachine.ChangeState(bossStateMachine.EnrageState);
      }
    }
    else if (ragePercentage > 0.01f && ragePercentage <= 0.30f)
    {
      if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.AlmostFullState))
      {
        bossStateMachine.ChangeState(bossStateMachine.AlmostFullState);
      }
    }
    else if (ragePercentage <= 0.01f)
    {
      if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.FullState))
      {
        bossStateMachine.ChangeState(bossStateMachine.FullState);
      }
    }
    else
    {
      if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.IdleState))
      {
        bossStateMachine.ChangeState(bossStateMachine.IdleState);
      }
    }
  }

  private float getRagePointPercentage()
  {
    return _rage / MAX_RAGE;
  }

  public bool OnHit(Items hitObj)
  {
    switch(hitObj)
    {
      case Product product:
        UpdateRageValue(-product.GetDamage());
        return true;
      default:
        return false;
    }
  }
}