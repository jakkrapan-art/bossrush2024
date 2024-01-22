using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Plant : Entity
{
  public bool isWet { get; private set; } = false;
  public bool isFertilized { get; private set; } = false;

  [field: SerializeField] public Animator _animator { get; private set; } = null;

  [SerializeField]
  private Image _growProgressUI = default;

  public bool IsReadyToGrow() => isWet && isFertilized;

  private void Awake()
  {
    _animator = GetComponent<Animator>();
  }

  private void Start()
  {
    _stateMachine = new PlantStateMachine(this);
  }

  private void Update()
  {
    if(_stateMachine != null) _stateMachine.Update();

    if(Input.GetKeyDown(KeyCode.Alpha0))
    {
      FillWater();
    }

    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      AddFertilizer();
    }
  }

  private void FixedUpdate()
  {
    if(_stateMachine != null) _stateMachine.FixedUpdate();
  }

  public void FillWater()
  {
    isWet = true;
  }

  public void AddFertilizer()
  {
    isFertilized = true;
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

  public PlantData GetPlantData()
  {
    return (PlantData) _data;
  }
}
