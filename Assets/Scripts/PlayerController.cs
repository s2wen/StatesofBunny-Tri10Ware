using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions controls;
    
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

    private void Move(Vector2 direction){
        if(canMove(direction)) transform.position += (Vector3)direction;
        Debug.Log("move " + direction);
    }

    private bool canMove(Vector2 direction){
        
        return true; //TODO: check player state and which tile is being moved into
    }
}
