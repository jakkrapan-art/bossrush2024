using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class Boss : Entity, IHitableObject
{
  private readonly float MAX_RAGE = 100;
  [SerializeField]
  private float _rage = 50.00f;
  [SerializeField]
  private float _ragePointIncreased = 0.16f;
  private BossSkillController _skillController = null;
  private BossStomach _stomach = null;

  [SerializeField]
  private GameObject _thinkingBubble = null;

  private UIBar _bossRageBar = null;
  private FoodRequest _foodRequest = null;
  protected override void Awake()
  {
    base.Awake();
    BossData bossData = GetBossData();
    _stateMachine = new BossStateMachine(this);
    _skillController = new BossSkillController(this);
    _stomach = new BossStomach(bossData.MaxPlant);

    SetupPossibleRequestFood();
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

    HideThinking();
  }

  protected override void Update()
  {
    UpdateRageValue(_ragePointIncreased * Time.deltaTime);
    _stateMachine?.Update();
    if (_skillController != null && _skillController.CanUse()) _skillController.TriggerSkill(_rage);
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
        return DoEat(product);
      default:
        return false;
    }
  }

  public void ShowThinking(Action callback)
  {
    StartCoroutine(DoShowThinking(4, callback));
  }

  private IEnumerator DoShowThinking(float second, Action callback)
  {
    SetActiveThinkingBubble(true);
    yield return new WaitForSeconds(second);
    callback?.Invoke();
    HideThinking();
  }

  public void HideThinking()
  {
    SetActiveThinkingBubble(false);
  }

  private void SetActiveThinkingBubble(bool active)
  {
    if (_thinkingBubble == null) return;

    _thinkingBubble.SetActive(active);
  }

  private bool DoEat(Product food)
  {
    if (_stomach != null)
    {
      var eatResult = _stomach.Eat(food);
      if(eatResult == BossStomach.EatResult.Think || eatResult == BossStomach.EatResult.Eat)
      {
        if (eatResult == BossStomach.EatResult.Think) ShowThinking(_stomach.RandomRequestFood);

        UpdateRageValue(-food.GetDamage());
        return true;
      }
      else
      {
        return false;
      }
    }

    return false;
  }

  private void SetupPossibleRequestFood()
  {
    var foodList = GetBossData().PossibleRequestFoods;
    if (_stomach == null) return;
    _stomach.SetupRequestFoods(foodList);
  }

  private BossData GetBossData()
  {
    if (_data is BossData == false) throw new Exception("Data in Boss " + name + " is not a type of BossData.");
    return (BossData)_data;
  }
}