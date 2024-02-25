using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Plant : InteractObject, IPoolingObject
{
  public bool _isWet { get; private set; } = false;
  public bool _isFertilized { get; private set; } = false;

  [field: SerializeField] public Product Product { get; private set; }
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

  private Action<Product> _onFullyGrowth = null;

  public bool IsReadyToGrow() => _isWet && _isFertilized;
  private Items _interactingItem;

  protected override void Awake()
  {
    base.Awake();

    _animator = GetComponent<Animator>();
    _stateMachine = new PlantStateMachine(this);
  }

  private void Update()
  {
    _stateMachine?.Update();
  }

  private void FixedUpdate()
  {
    _stateMachine?.FixedUpdate();
  }

  public void Setup(Product product, Action<Product> onFullyGrowth)
  {
    Product = product;
    _onFullyGrowth = onFullyGrowth;
    _interactingItem = null;
    _isFertilized = false;
    _isWet = false;

    SetActiveNeedFertilizedUI(true);
    SetActiveNeedWaterUI(true);

    _stateMachine = new PlantStateMachine(this);
  }

  public void OnFullyGrowth()
  {
    var product = ObjectPool.GetInstance().Get<Product>(Product.name);
    product.transform.position = transform.position;
    _onFullyGrowth?.Invoke(product);
    //return to pool process
    ObjectPool.ReturnObjectToPool(this);
  }

  public void FillWater()
  {
    _isWet = true;
    SetActiveNeedWaterUI(false);
  }

  public void AddFertilizer()
  {
    _isFertilized = true;
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

  public override void InteractResult()
  {
    Debug.Log("interact result at current state = " + _stateMachine?.CurrentState ?? "N/A");
    if(_stateMachine.CurrentState is PlantSeedState)
    {
      if (_interactingItem)
      {
        switch (_interactingItem)
        {
          case WaterCan:
            FillWater();
            break;
          case Fertilizer fertilizer:
            if (!_isFertilized)
            {
              AddFertilizer();
              fertilizer.Use();
            }
            break;
        }
      }
    }
  }

  public void ResetPoolingObject()
  {
  }
}
