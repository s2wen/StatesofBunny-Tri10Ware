using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color closedColor = Color.gray;
    [SerializeField] private Color openColor = Color.white;

    [SerializeField] private string nextScene;
    public bool IsCompleted { get; private set; }
    private PlayerController player;
    
    public bool IsOpen { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        player = FindObjectsByType<PlayerController>()[0];
        if (!levelLoader) levelLoader = FindAnyObjectByType<LevelLoader>();
        SetOpen(false);
    }


    public void SetOpen(bool open)
    {
        IsOpen = open;
        if (sprite){
            sprite.color = open ? openColor : closedColor;
        } 
    }

    public void ReachExit()
    {
        if(!IsOpen || IsCompleted) return;
        Vector2Int curPos = Vector2Int.RoundToInt(transform.position);
        
        if (Vector2Int.RoundToInt(player.transform.position) == curPos)
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1)
            {
                IsCompleted = true;
                Debug.Log("All levels completed!");
                return;
            }
            if (levelLoader) levelLoader.LoadLevel(nextScene);
            else if (!string.IsNullOrEmpty(nextScene)) SceneManager.LoadScene(nextScene);
            else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        }
        
        
    }

}
