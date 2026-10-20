using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class Push : MonoBehaviour
{
    public static event System.Action BlockSettled;
    
    [SerializeField]
    private Tilemap tilemap;

    private GameObject[] Blocks;
    private GameObject[] Vents;
    
    private float duration = 0.1f;
    private bool isMoving;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Blocks = GameObject.FindGameObjectsWithTag("Blocks");
        Vents = GameObject.FindGameObjectsWithTag("Vents");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Move(Vector2 direction){
        if (isMoving) return false;

        if(canMove(direction)){
            StartCoroutine(MoveRoutine(direction));
            return true;
        }else{
            
            return false;
        }
    }

    private bool canMove(Vector2 direction){

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)direction;

        foreach(var v in Vents){
            if(endPos.x==v.transform.position.x && endPos.y==v.transform.position.y){
                return false;
            }
        }

        foreach(var b in Blocks){
            if(endPos.x==b.transform.position.x && endPos.y==b.transform.position.y){
                return false;
                
            }
        }

        Vector3Int targetTile = tilemap.WorldToCell((Vector3Int)Vector3Int.RoundToInt(endPos));
        if (!tilemap.HasTile(targetTile))
        {
            return false;
        }

        return true;
    }

    private IEnumerator MoveRoutine(Vector2 direction)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)direction;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            yield return null;
        }

        transform.position = endPos;
        isMoving = false;

        BlockSettled?.Invoke();
    }
}
