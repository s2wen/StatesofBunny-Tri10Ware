using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class Push : MonoBehaviour
{
    public static event System.Action BlockSettled;
    
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField] private bool fillsHoles;

    public bool IsBridge { get; private set; }
    public bool FillsHoles => fillsHoles;

    public static bool HasBridgeAt(Vector2Int position, GameObject[] blocks)
    {
        foreach (var block in blocks)
        {
            if (block && block.TryGetComponent(out Push push) && push.IsBridge
                && Vector2Int.RoundToInt(block.transform.position) == position)
                return true;
        }
        return false;
    }

    private GameObject[] Blocks;
    private GameObject[] Vents;
    
    private float duration = 0.1f;
    private bool isMoving;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!tilemap)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player) tilemap = player.FloorTilemap;
        }
        Blocks = GameObject.FindGameObjectsWithTag("Blocks");
        Vents = GameObject.FindGameObjectsWithTag("Vents");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Move(Vector2 direction){
        if (isMoving || IsBridge || !tilemap) return false;

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
        Vector2Int target = Vector2Int.RoundToInt(endPos);
        if (SolidWall.IsAt(target)) return false;
        var board = tilemap.GetComponent<LevelBoard>();
        if ((board && !board.Contains(target)) || PermeableWall.Blocks(target, States.Solid, true))
            return false;

        foreach(var v in Vents){
            if(endPos.x==v.transform.position.x && endPos.y==v.transform.position.y){
                return false;
            }
        }

        foreach(var b in Blocks){
            if (b.TryGetComponent(out Push push) && push.IsBridge) continue;
            if(endPos.x==b.transform.position.x && endPos.y==b.transform.position.y){
                return false;
                
            }
        }

        Vector3Int targetTile = tilemap.WorldToCell((Vector3Int)Vector3Int.RoundToInt(endPos));
        if ((!tilemap.HasTile(targetTile) || Hole.IsHoleAt(Vector2Int.RoundToInt(endPos)))
            && !HasBridgeAt(Vector2Int.RoundToInt(endPos), Blocks) && !fillsHoles)
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

        Vector3Int cell = tilemap.WorldToCell((Vector3Int)Vector3Int.RoundToInt(endPos));
        if (fillsHoles && (!tilemap.HasTile(cell) || Hole.IsHoleAt(Vector2Int.RoundToInt(endPos)))
            && !HasBridgeAt(Vector2Int.RoundToInt(endPos), Blocks))
        {
            IsBridge = true;
            Hole.CoverAt(Vector2Int.RoundToInt(endPos));
            foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
            if (TryGetComponent(out Rigidbody2D body)) body.bodyType = RigidbodyType2D.Static;
        }
        isMoving = false;

        BlockSettled?.Invoke();
    }
}
