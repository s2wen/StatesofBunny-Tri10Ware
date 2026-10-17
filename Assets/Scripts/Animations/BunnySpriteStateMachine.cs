using UnityEngine;

public class BunnySpriteStateMachine
{
    private PlayerAttributes playerState;

    protected Animator animationController;
}

public class BunnyIdleState : BunnySpriteStateMachine
{
    public void Enter()
    {
        animationController.Play("Idle");
    }

    public void Update() { }
}

public class BunnyWalkingState : BunnySpriteStateMachine
{
    public void Enter()
    {
        animationController.Play("Walking");
    }

    public void Update() { }
}
