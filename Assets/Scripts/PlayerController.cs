using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private GameObject[] Blocks;
    private GameObject[] Vents;

    private InputSystem_Actions controls;
    private float duration = 0.1f;
    private bool isMoving;

    private PlayerAttributes playerState;
    private SpriteRenderer spriteRenderer;


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
        Vents = GameObject.FindGameObjectsWithTag("Vents");
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
        if (isMoving) return;

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

        if (playerState.getFlight())
        {
            //smth to do with moving balloons here or whatever

            return true;
        }

        foreach (var v in Vents)
        {
            if (Vector2Int.RoundToInt(v.transform.position) == target)
            {
                if (playerState.getSize() == 0.5f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        foreach (var b in Blocks)
        {
            if (Vector2Int.RoundToInt(b.transform.position) == target)
            {
                foreach (var b in Blocks)
                {
                    if (Vector2Int.RoundToInt(b.transform.position) == target)
                    {
                        Push blockPush = b.GetComponent<Push>();
                        if (playerState.getStrength() == 1 && blockPush && blockPush.Move(direction))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
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
    void OnTriggerEnter(BoxCollider2D other)
    {
        string zoneTag = other.tag;
        States currentState = PlayerAttributes.

        switch (zoneTag)
        {
            case "Cooler":
                break;
            case "Heater":
                break;
        }
    }
}
