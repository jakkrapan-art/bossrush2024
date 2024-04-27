using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
  [SerializeField]
  private Game gameControllerTemplate = null;

  private void Start()
  {
    if(gameControllerTemplate != null)
    {
      var gameController = Instantiate(gameControllerTemplate);

      gameController.Setup(ShowGameEndDialog, ShowGameEndDialog);
    }
  }

  private void ShowGameEndDialog()
  {
    UICreator uiCreator = UICreator.GetInstance();
    uiCreator.CreateGameEndDialog((ui) =>
    {
      if (ui == null) return;
      ui.Setup(()=> SceneManager.LoadScene(Const.GAME_SCENE_NAME, LoadSceneMode.Single));
    });
  }
}
