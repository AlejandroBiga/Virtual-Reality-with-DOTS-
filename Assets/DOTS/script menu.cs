using UnityEngine;
using UnityEngine.SceneManagement;

public class scriptmenu : MonoBehaviour
{
   public void PlayButton()
    {
        SceneManager.LoadScene("Lvl scene");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
