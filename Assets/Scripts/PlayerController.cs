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

    private InputSystem_Actions controls;
    private float duration = 0.1f;
    private bool isMoving;

    private PlayerAttributes playerState;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField]
    private Tilemap tilemap;

    private Drain destination;

    private string previousAnim;
    private const string SolidFrontAnim = "front_solid_animation";
    private const string SolidSideAnim = "side_solid_animation";
    private const string LiquidFrontAnim = "front_liquid_animation";
    private const string LiquidSideAnim = "side_liquid_animation";
    private const string GasFrontAnim = "front_gas_animation";
    private const string GasSideAnim = "side_gas_animation";

    void Awake()
    {
        controls = new InputSystem_Actions();

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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
        Airflows = GameObject.FindGameObjectsWithTag("Airflows");
        playerState = GetComponent<PlayerAttributes>();
        controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Player.Interact.performed += ctx => ChangeState();
        previousAnim = GasFrontAnim;
    }

    private void ChangeState()
    {
        playerState.setState((States)(((int)playerState.getState() + 1) % 3));
        spriteRenderer.color = playerState.getColor();
        Debug.Log("Current State:" + playerState.getState());
    }

    /**
        animated movement
    **/
    private void Move(Vector2 direction)
    {
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

        foreach (var b in Blocks)
        {
            if (Vector2Int.RoundToInt(b.transform.position) == target)
            {
                Push blockPush = b.GetComponent<Push>();
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
        if (!tilemap.HasTile(targetTile) && playerState.getState() != States.Gas)
        {
            Debug.Log("Can't walk over void");
            return false;
        }

        return true; //TODO: check player state and which tile is being moved into
    }

    private void handleAnimation(Vector2 direction)
    {
        string anim = previousAnim;
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x > 0;
            spriteRenderer.flipY = false;
            switch (playerState.getState())
            {
                case States.Solid:
                    anim = SolidSideAnim;
                    break;
                case States.Liquid:
                    anim = LiquidSideAnim;
                    break;
                case States.Gas:
                    anim = GasSideAnim;
                    break;
                default:
                    break;
            }
        }
        else if (direction.y != 0)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = direction.y > 0;
            switch (playerState.getState())
            {
                case States.Solid:
                    anim = SolidFrontAnim;
                    break;
                case States.Liquid:
                    anim = LiquidFrontAnim;
                    break;
                case States.Gas:
                    anim = GasFrontAnim;
                    break;
                default:
                    break;
            }
        }

        AnimatorStateInfo currentAnim = animator.GetCurrentAnimatorStateInfo(0);
        if (!currentAnim.IsName(anim))
            animator.Play(anim);
        previousAnim = anim;
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
            handleAnimation(direction);
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
                    transform.position = new Vector3(
                        destination.getPos().x,
                        destination.getPos().y,
                        transform.position.z
                    );
                }
                break;
        }
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
