
using UnityEngine;

public class Oven : InteractableObject
{
  [SerializeField]
  private Animator _animator = default;

  private bool _isCooking = false;
  private Product[] _itemInput = new Product[3];
  private int _currentIndex = 0;

  private OvenStateMachine _stateMachine;

  public Animator GetAnimator() => _animator;

  protected override void Awake()
  {
    base.Awake();
    _stateMachine = new OvenStateMachine(this);
  }


  public override ResultData Interact(Item interactingItem)
  {
    if(!_isCooking && interactingItem != null && interactingItem is Product prod) 
    {
      AddMaterial(prod);
      return new ResultData { clearHand = true };
    }

    return new ResultData { waitTime = 0 };
  }

  private void AddMaterial(Product product)
  {
    if (_currentIndex >= _itemInput.Length) return;
    _itemInput[_currentIndex++] = product;

    if (_currentIndex == _itemInput.Length)
    {
      Cook();
      _currentIndex = 0;
    }
  }

  private void Cook()
  {
    _isCooking = true;
  }
}
