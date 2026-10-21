using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;
    public bool IsLoading { get; private set; }

    void Awake()
    {
        if (transition) transition.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    public void LoadNextLevel() => LoadLevel(null);

    // Keep named destinations (including Level2.5), with the branch's next-index fallback.
    public void LoadLevel(string sceneName)
    {
        if (IsLoading) return;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool named = !string.IsNullOrEmpty(sceneName);
        if (!(named ? Application.CanStreamedLevelBeLoaded(sceneName) : Application.CanStreamedLevelBeLoaded(nextIndex)))
        {
            Debug.LogError("Cannot load level: " + (named ? sceneName : nextIndex.ToString()));
            return;
        }
        IsLoading = true;
        StartCoroutine(LoadLevelRoutine(sceneName, nextIndex));
    }

    IEnumerator LoadLevelRoutine(string sceneName, int nextIndex)
    {
        if (transition)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSecondsRealtime(Mathf.Max(0, transitionTime));
            // Allow the last animation frame to reach full black before replacing the scene.
            yield return null;
        }
        if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
        else SceneManager.LoadScene(nextIndex);
    }
}
