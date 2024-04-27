using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
  [SerializeField]
  private FoodRecipeList recipeList = null;
  //database obj
  private FoodRecipeDB _recipeDB = null;

  [Header("Cooking System")]
  [SerializeField]
  private Oven _ovenTemplate = default;
  [SerializeField]
  private Vector3 _ovenPos = default;

  [Header("Boss")]
  [SerializeField]
  private Boss _bossTemplate = default;
  [SerializeField]
  private Vector3 _bossPos = default;

  private bool _isEnded = false;

  public void Setup(Action onWinGame, Action onLoseGame)
  {
    _recipeDB = new FoodRecipeDB(recipeList.Recipes);

    //create oven
    var oven = Instantiate(_ovenTemplate, _ovenPos, Quaternion.identity);
    oven.Setup(_recipeDB);

    //create boss
    var boss = Instantiate(_bossTemplate, _bossPos, Quaternion.identity);
    boss.Setup(() => 
    {
      return _isEnded;
    },_recipeDB, () => 
    {
      _isEnded = true;
      onLoseGame?.Invoke();
    }, () => 
    {
      _isEnded = true;
      onWinGame?.Invoke();
    });
  }
}
