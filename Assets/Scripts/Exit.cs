using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Exit : MonoBehaviour
{
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color closedColor = Color.gray;
    [SerializeField] private Color openColor = Color.white;
    [SerializeField] private AudioClip portalSound;

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
        {
            StartCoroutine(WaitForSoundAndLoadScene());
        }
    }

    private IEnumerator WaitForSoundAndLoadScene()
    {
        AudioSource.PlayClipAtPoint(portalSound, transform.position);
        player.movementLockedUntil = Time.time + portalSound.length;
        yield return new WaitForSeconds(portalSound.length);
        levelLoader.LoadNextLevel();
    }

}
