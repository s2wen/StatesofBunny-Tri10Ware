using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("PlayerMovement");
    }

    public void Credits()
    {
        SceneManager.LoadSceneAsync("Credits");
    }

    public void MainMenuButton()
    {
        SceneManager.LoadSceneAsync("Main Menu");
    }

}
