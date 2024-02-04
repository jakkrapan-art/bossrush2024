using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Plant : Items
{
  public bool isWet { get; private set; } = false;
  public bool isFertilized { get; private set; } = false;

  [field: SerializeField] public Items Product { get; }
  [field: SerializeField] public Animator _animator { get; private set; } = null;

  private PlantStateMachine _stateMachine = null;
  [SerializeField]
  private PlantData _data = null;

  [Header("UI"), SerializeField]
  private Image _growProgressUI = default;
  [SerializeField]
  private Image _needWaterUI = default;
  [SerializeField]
  private Image _needFertilizerUI = default;

  private Soil _plantingSoil;
  public bool IsReadyToGrow() => isWet && isFertilized;
  private Items _interactingItem;

  protected override void Awake()
  {
    base.Awake();

    _animator = GetComponent<Animator>();
    _stateMachine = new PlantStateMachine(this);
  }

  private void Start()
  {
    SetActiveNeedFertilizedUI(true);
    SetActiveNeedWaterUI(true);
  }

  protected override void Update()
  {
    base.Update();
    if(_stateMachine != null) _stateMachine.Update();
  }

  private void FixedUpdate()
  {
    if(_stateMachine != null) _stateMachine.FixedUpdate();
  }

  public void SetPlantingSoil(Soil soil)
  {
    _plantingSoil = soil;
  }

  public void FillWater()
  {
    isWet = true;
    SetActiveNeedWaterUI(false);
  }

  public void AddFertilizer()
  {
    isFertilized = true;
    SetActiveNeedFertilizedUI(false);
  }

  public void SetActiveUIGrowProgress(bool active)
  {
    if (_growProgressUI) _growProgressUI.gameObject.SetActive(active);
  }

  public void UpdateUIGrowProgressValue(float value)
  {
    if(_growProgressUI) _growProgressUI.fillAmount = value;
  }

  public void SetAnimatorBool(string paramName, bool value)
  {
    if(_animator) _animator.SetBool(paramName, value);
  }

  private void SetActiveNeedWaterUI(bool active)
  {
    if(_needWaterUI) _needWaterUI.gameObject.SetActive(active);
  }

  private void SetActiveNeedFertilizedUI(bool active)
  {
    if(_needFertilizerUI) _needFertilizerUI.gameObject.SetActive(active);
  }

  public PlantData GetPlantData()
  {
    return _data;
  }

  public State GetCurrentState() => _stateMachine.GetCurrentState();

  public override bool CanInteract(Items itemToInteract)
  {
    _interactingItem = itemToInteract;
    return true;
  }

  public override void Kept(GameObject objectHand)
  {
    if (_stateMachine.CurrentState is PlantMatureState == false) return;
    base.Kept(objectHand);
  }

  public override void InteractResult()
  {
    switch(_stateMachine.CurrentState)
    {
      case PlantMatureState:
        base.InteractResult();
        break;
      case PlantSeedState:
        if(_interactingItem)
        {
          switch(_interactingItem)
          {
            case WaterCan:
              FillWater();
              break;
            case Fertilizer fertilizer:
              if(!isFertilized)
              {
                AddFertilizer();
                fertilizer.Use();
              }
              break;
          }
        }
        break;
    }
  }
}
