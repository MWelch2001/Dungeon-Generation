using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public void ToMain()
    {
        SceneManager.LoadSceneAsync("Scenes/Main-Menu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Scenes/Dungeon");
    }


}
