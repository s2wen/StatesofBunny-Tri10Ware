using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color closedColor = Color.gray;
    [SerializeField] private Color openColor = Color.white;

    private PlayerController player;
    
    public bool IsOpen { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        player = FindObjectsByType<PlayerController>()[0];
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
        if(!IsOpen) return;
        Vector2Int curPos = Vector2Int.RoundToInt(transform.position);
        
        if (Vector2Int.RoundToInt(player.transform.position) == curPos)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        
        
    }

}
