using UnityEngine;
using UnityEngine.SceneManagement;

public class scriptmenu : MonoBehaviour
{
   public void PlayButton()
    {
        SceneManager.LoadScene("loadingscene");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
