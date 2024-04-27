using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGameEnd : UIDialogBase
{
  public void Setup(Action playAgain)
  {
    AddCloseListener(playAgain);
  }
}
