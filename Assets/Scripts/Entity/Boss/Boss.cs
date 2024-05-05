using System;
using System.Collections;
using UnityEngine;

public class Boss : Entity, IHitableObject
{
  private readonly float MAX_RAGE = 100;
  [SerializeField]
  private float _rage = 50.00f;
  [SerializeField]
  private float _ragePointIncreased = 0.16f;
  [field: SerializeField]
  public Animator Animator { get; private set; } = null;
  private BossSkillController _skillController = null;
  private BossStomach _stomach = null;

  [Header("Skill")]
  [SerializeField]
  private AbilityHolder _bossAbilities = null;

  [Header("Bubbles")]
  [SerializeField]
  private UIBossParticle _thinkingBubble = null;
  [SerializeField]
  private UIFoodRequestBubble _foodRequestBubble = null;

  private UIBar _bossRageBar = null;

  private Action _onRageMax = null;
  private Action _onRageMin = null;

  private Func<bool> _isEnded = null;

  protected override void Update()
  {
    if (_isEnded?.Invoke() ?? false) return;
    UpdateRageValue(_ragePointIncreased * Time.deltaTime);
    _stateMachine?.Update();
    if (_skillController != null && _skillController.CanUse()) _skillController.TriggerSkill(_rage);

    ActiveAbility();
  }

  private void FixedUpdate()
  {
    if (_isEnded?.Invoke() ?? false) return;
    _stateMachine?.FixedUpdate();
  }

  private void ActiveAbility()
  {
    if (_bossAbilities == null || (_bossAbilities != null && !_bossAbilities.IsReady())) return;

    _bossAbilities.ActiveAbility(_rage);
  }

  private void UpdateRageValue(float updateValue)
  {
    _rage = Mathf.Clamp(_rage + updateValue, 0, MAX_RAGE);

    if (_rage <= 0)
    {
      _onRageMin?.Invoke();
      return;
    }
    else if(_rage >= MAX_RAGE)
    {
      _onRageMax?.Invoke();
      return;
    }


    handleStateByRagePoint();

    if (_bossRageBar)
    {
      _bossRageBar.UpdateValue(getRagePointPercentage());
    }
  }

  public void Setup(Func<bool> isEndCheck, FoodRecipeDB recipeDB, Action onRageMin, Action onRageMax)
  {
    _isEnded = isEndCheck;

    UICreator.GetInstance().CreateBossRageBar((bar) =>
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

    BossData bossData = GetBossData();
    _stateMachine = new BossStateMachine(this);
    _skillController = new BossSkillController(this);
    _stomach = new BossStomach(bossData.MaxPlant);
    SetupPossibleRequestFood(recipeDB);

    _onRageMin = onRageMin;
    _onRageMax = onRageMax;
  }

  private void StartRageIncreaseTimer()
  {

  }

  private void handleStateByRagePoint()
  {
    if (_stateMachine.GetCurrentState() is BossEatState) return;

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

  public bool OnHit(Item hitObj)
  {
    switch(hitObj)
    {
      case Product product:
        return Eat(product);
      default:
        return false;
    }
  }

  public void ShowThinking(Action callback)
  {
    SetActiveThinkingBubble(true, 4, callback);
  }

  public void HideThinking()
  {
    SetActiveThinkingBubble(false);
  }

  private void SetActiveThinkingBubble(bool active, float second = 0, Action callback = null)
  {
    if (_thinkingBubble == null) return;
    if (active)
    {
      _thinkingBubble.Show(second, callback);
    }
    else
    {
      _thinkingBubble.Hide();
    }
  }

  private void ShowFoodRequestBubble(Sprite foodIcon)
  {
    if (_foodRequestBubble == null) return;
    _foodRequestBubble.SetFoodImage(foodIcon);
    _foodRequestBubble.Show(-1);
  }

  private void HideFoodRequestBubble()
  {
    if (_foodRequestBubble == null) return;
    _foodRequestBubble.Hide();
  }

  private bool Eat(Product food)
  {
    if (!food.GetProductData().Eatable) return false;

    if (_stomach != null)
    {
      var eatResult = _stomach.Eat(food);
      if(eatResult == BossStomach.EatResult.Think || eatResult == BossStomach.EatResult.Eat)
      {
        HideFoodRequestBubble();

        _stateMachine.ChangeState(GetStateMachine().EatState);

        if (eatResult == BossStomach.EatResult.Think) ShowThinking(()=>
        {
          _stomach.RandomRequestFood((result) => 
          {
            ShowFoodRequestBubble(result.GetIconSprite());
          });
        });

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

  private void SetupPossibleRequestFood(FoodRecipeDB recipeDB)
  {
    var foodList = GetBossData().PossibleRequestFoods;
    if (_stomach == null) return;
    _stomach.Setup(foodList, recipeDB);
  }

  private BossData GetBossData()
  {
    if (_data is BossData == false) throw new Exception("Data in Boss " + name + " is not a type of BossData.");
    return (BossData)_data;
  }

  private BossStateMachine GetStateMachine() => (BossStateMachine) _stateMachine;
}