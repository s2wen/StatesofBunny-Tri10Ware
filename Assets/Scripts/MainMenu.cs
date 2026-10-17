using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject creditsButton;
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Level1");
    }

    public void Credits()
    {
        SceneManager.LoadSceneAsync("Credits");
    }

    void Start()
    {
        playButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        creditsButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

}
