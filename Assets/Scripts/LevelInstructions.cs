using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelInstructions : MonoBehaviour
{
    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
