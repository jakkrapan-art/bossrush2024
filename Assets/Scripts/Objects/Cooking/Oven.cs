
using System;
using System.Collections;
using UnityEngine;

public class Oven : InteractableObject
{
  [SerializeField]
  private Animator _animator = default;

  [SerializeField]
  private SpriteRenderer _foodOutput = default;

  [SerializeField]
  private SpriteRenderer _itemInputSr = null;
  private Coroutine _addMaterialCoroutine = null;

  private bool _isCooking = false;
  private Product[] _itemInput = new Product[3];
  private int _currentIndex = 0;

  private OvenStateMachine _stateMachine;
  public float _cookTime { get; private set; }

  public Animator GetAnimator() => _animator;
  private FoodRecipeDB _foodRecipeDB;
  private Product _cookingProduct = null;

  private struct ProcessFoodResult
  {
    public float CookTime { get; private set; }
    public Product Food { get; private set; }

    public ProcessFoodResult(float cookTime, Product food)
    {
      CookTime = cookTime;
      Food = food;
    }
  }

  protected override void Awake()
  {
    base.Awake();
    _stateMachine = new OvenStateMachine(this);
  }

  private void Update()
  {
    _stateMachine?.Update();
  }

  private void FixedUpdate()
  {
    _stateMachine?.FixedUpdate();
  }

  public void Setup(FoodRecipeDB recipeDB)
  {
    _foodRecipeDB = recipeDB;

    HideOutput();
  }

  public override InteractResultData Interact(Item interactingItem)
  {
    if(_ItemsOutput != null)
    {
      var output = ObjectPool.GetInstance().Get<Product>(_ItemsOutput.name);
      _ItemsOutput = null;
      HideOutput();
      return new InteractResultData { returnItem = output };
    }
    else if(!_isCooking && interactingItem != null && interactingItem is Product prod) 
    {
      AddMaterial(prod);
      return new InteractResultData { clearHand = true };
    }

    return new InteractResultData { waitTime = 0 };
  }

  private void AddMaterial(Product product)
  {
    if (_currentIndex >= _itemInput.Length) return;
    _itemInput[_currentIndex++] = product;

    if (_addMaterialCoroutine != null) StopCoroutine(_addMaterialCoroutine);

    _addMaterialCoroutine = StartCoroutine(SlowAddMaterial(product, () =>
    {
      if (_currentIndex == _itemInput.Length)
      {
        Cook();
        _currentIndex = 0;
      }
    }));
  }

  private IEnumerator SlowAddMaterial(Product product, Action callback)
  {
    float estimateTimeUsed = 1.5f;
    if(_itemInputSr != null) 
    {
      _itemInputSr.sprite = product.GetIconSprite();
      _itemInputSr.gameObject.SetActive(true);

      var itemTransform = _itemInputSr.transform;
      itemTransform.localPosition = new Vector2(0, 4.5f);
      itemTransform.localScale = new Vector2(3f, 3f);

      var targetPos = new Vector3(0, 1.6f);
      var start = Time.time;
      var interpolates = 0.02f;
      while (start + estimateTimeUsed > Time.time)
      {
        var newPos = Vector3.Lerp(itemTransform.localPosition, targetPos, interpolates);
        var newScale = Vector3.Lerp(itemTransform.localScale, new Vector3(1, 1, 1), interpolates);

        itemTransform.localPosition = newPos;
        itemTransform.localScale = newScale;

        yield return new WaitForFixedUpdate();
      }
    }

    _itemInputSr.gameObject.SetActive(false);
    callback?.Invoke();
  }

  private void Cook()
  {
    //determine the receipe and get cook time
    FoodRecipe recipe = ProcessFoodReceipe();
    if (recipe == null) 
    {
      Debug.LogWarning("Not found any recipe for input");
      ClearInput();
      return;
    }

    _cookTime = recipe.CookTime;
    _cookingProduct = recipe.Food;

    _isCooking = true;
    if(_stateMachine != null) _stateMachine.ChangeState(_stateMachine.CookState);
  }

  public FoodRecipe ProcessFoodReceipe()
  {
    if (_foodRecipeDB == null) return null;

    var recipe = _foodRecipeDB.GetRecipe(_itemInput);
    if(recipe == null) return null;

    ClearInput();

    return recipe;
  }

  public void CookSuccess()
  {
    ShowOutput(_cookingProduct.GetIconSprite());
    _ItemsOutput = _cookingProduct;
    _cookingProduct = null;

    _isCooking = false;
  }

  private void ClearInput()
  {
    for (int i = 0; i < _itemInput.Length; i++)
    {
      _itemInput[i] = null;
    }
  }

  public void ShowOutput(Sprite foodSprite)
  {
    if (_foodOutput == null) return;

    _foodOutput.sprite = foodSprite;
    _foodOutput.gameObject.SetActive(true);
  }

  public void HideOutput()
  {
    if (_foodOutput == null) return;

    _foodOutput.gameObject.SetActive(false);
  }
}
