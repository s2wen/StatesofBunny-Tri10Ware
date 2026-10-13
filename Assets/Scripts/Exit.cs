using UnityEngine;

public class Exit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color closedColor = Color.gray;
    [SerializeField] private Color openColor = Color.white;
    
    public bool IsOpen { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() => SetOpen(false);


    public void SetOpen(bool open)
    {
        IsOpen = open;
        if (sprite){
            sprite.color = open ? openColor : closedColor;
        } 
    }

}
