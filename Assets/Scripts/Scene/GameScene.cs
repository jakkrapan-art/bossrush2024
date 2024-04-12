using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
  [SerializeField]
  private Game gameControllerTemplate = null;

  private void Start()
  {
    if(gameControllerTemplate != null)
    {
      var gameController = Instantiate(gameControllerTemplate);
      gameController.Setup();
    }
  }
}
