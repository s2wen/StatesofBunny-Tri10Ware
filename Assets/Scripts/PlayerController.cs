using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Exit exit;
    private GameObject[] Blocks;
    private GameObject[] Vents;
    private GameObject[] Airflows;
    private BlockTarget[] targets;

    private InputSystem_Actions controls;
    private float duration = 0.1f;
    private bool isMoving;

    private PlayerAttributes playerState;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Tilemap tilemap;

    public Tilemap FloorTilemap => tilemap;

    private Drain destination;


    void Awake()
    {
        controls = new InputSystem_Actions();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        Blocks = GameObject.FindGameObjectsWithTag("Blocks");
        targets = FindObjectsByType<BlockTarget>();
        Vents = GameObject.FindGameObjectsWithTag("Vents");
        Airflows = GameObject.FindGameObjectsWithTag("Airflows");
        Debug.Log($"AirFlows is {Airflows.Length}");
        playerState = GetComponent<PlayerAttributes>();
        Debug.Log("Current State: " + playerState.getState());
        controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Player.Interact.performed += ctx => ChangeState();
    }

    private void ChangeState()
    {
        playerState.setState((States)(((int)playerState.getState() + 1) % 3));
        spriteRenderer.color = playerState.getColor();
        Debug.Log("Current State:" + playerState.getState());
    }

    /**
        snap-to-tile movement w/o animation
    **/
    // private void Move(Vector2 direction){
    //     if(canMove(direction))  transform.position += (Vector3)direction;
    //     Debug.Log("move " + direction);
    // }

    /**
        animated movement
    **/
    private void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        // Only the new board enforces cardinal steps. Preserve the original
        // PlayerMovement scene's diagonal and analog input behavior.
        if (tilemap.GetComponent<LevelBoard>())
            direction = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0) : new Vector2(0, Mathf.Sign(direction.y));
        if (isMoving)
            return;

        if (canMove(direction))
        {
            StartCoroutine(MoveRoutine(direction)); //transform.position += (Vector3)direction;
        }
        else
        {
            StartCoroutine(FailMoveRoutine(direction));
        }
    }

    private bool canMove(Vector2 direction)
    {
        Vector2Int target = Vector2Int.RoundToInt((Vector2)transform.position + direction);
        if (SolidWall.IsAt(target)) return false;
        // Match PlayerMovement: missing floor blocks solid/liquid below,
        // while gas can cross empty space without an additional board/wall gate.

        /*
        if (playerState.getFlight())
        {
            //smth to do with moving balloons here or whatever

            return true;
        }
        */

        // foreach (var v in Vents)
        // {
        //     if (Vector2Int.RoundToInt(v.transform.position) == target)
        //     {
        //         if (playerState.getSize() == 0.5f)
        //         {
        //             return true;
        //         }
        //         else
        //         {
        //             return false;
        //         }
        //     }
        // }
        foreach (var b in Blocks)
        {
            if (Vector2Int.RoundToInt(b.transform.position) == target)
            {
                Push blockPush = b.GetComponent<Push>();
                if (blockPush && blockPush.IsBridge) continue;
                if (
                    playerState.getState() == States.Solid
                    && blockPush
                    && blockPush.Move(direction)
                )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // Only the gag scene enables movable targets. Any form can push these markers.
        foreach (var marker in targets)
            if (marker && marker.Movable && Vector2Int.RoundToInt(marker.transform.position) == target)
                return marker.Move(direction, tilemap, Blocks);

        // airflow check
        foreach (var a in Airflows)
        {
            if (
                Vector2Int.RoundToInt(a.transform.position) == target
                && playerState.getState() == States.Gas
            )
                return false;
        }

        // void check
        Vector3Int targetTile = tilemap.WorldToCell((Vector3Int)target);
        if ((!tilemap.HasTile(targetTile) || Hole.IsHoleAt(target)) && !Push.HasBridgeAt(target, Blocks)
            && playerState.getState() != States.Gas)
        {
            Debug.Log("Can't walk over void");
            return false;
        }

        return true; //TODO: check player state and which tile is being moved into
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

        exit.ReachExit();
    }

    private IEnumerator FailMoveRoutine(Vector2 direction)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + ((Vector3)direction * 0.3f);
        float elapsedTime = 0f;

        while (elapsedTime < duration * 2 / 3)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / (duration / 2));
            yield return null;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(endPos, startPos, elapsedTime / (duration / 2));
            yield return null;
        }

        transform.position = startPos;
        isMoving = false;
    }

    //Controls the trigger zones: ie, when entering a zone it triggers the effect
    void OnTriggerEnter2D(Collider2D other)
    {
        string zoneTag = other.tag;
        States currentState = playerState.getState();

        switch (zoneTag)
        {
            case "Cooler":
                if (currentState == States.Gas)
                {
                    playerState.setState(States.Liquid);
                }
                else if (currentState == States.Liquid)
                {
                    playerState.setState(States.Solid);
                }
                break;
            case "Heater":
                if (currentState == States.Liquid)
                {
                    playerState.setState(States.Gas);
                }
                else if (currentState == States.Solid)
                {
                    playerState.setState(States.Liquid);
                }
                break;
            case "Drain":
                if (currentState == States.Liquid && other.TryGetComponent(out Drain drain))
                {
                    if (drain == destination)
                    {
                        break;
                    }
                    destination = drain.moveNext();
                    StopAllCoroutines();
                    isMoving = false;
                    transform.position = new Vector3(destination.getPos().x, destination.getPos().y, transform.position.z);
                }
                break;
        }
        spriteRenderer.color = playerState.getColor();
        Debug.Log($"Changed state to {playerState.getState()}");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Drain drain) && drain == destination)
        {
            destination = null;
        }
    }
}
