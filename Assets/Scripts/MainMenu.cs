using TMPro; // Required if you need to reference TMP text elements specifically
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for the Button component

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button startButton; // should never be null

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        Debug.Log("I will now load the level");
        // SceneManager.LoadScene("Level", LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(StartGame);
    }
}
