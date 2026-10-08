using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions controls;
    private float duration = 0.2f;
    private bool isMoving;


    void Awake()
    {
        controls = new InputSystem_Actions();
    }

    private void OnEnable(){
        controls.Enable();
    }

    private void OnDisable(){
        controls.Disable();
    }

    void Start()
    {
        controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
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
    private void Move(Vector2 direction){
        if (isMoving) return;

        if(canMove(direction)) StartCoroutine(MoveRoutine(direction)); //transform.position += (Vector3)direction;
        Debug.Log("move " + direction);
    }

    
    private bool canMove(Vector2 direction){
        
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
}
