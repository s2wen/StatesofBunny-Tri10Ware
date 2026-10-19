using UnityEngine;

/// <summary>Attach to a consumable object with a Collider2D.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class DestroyOnPlayerTouch : MonoBehaviour
{
    private bool consumed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        ConsumeIfPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ConsumeIfPlayer(collision.collider);
    }

    private void ConsumeIfPlayer(Collider2D other)
    {
        // Also supports colliders placed on a child of the player object.
        if (consumed || !other.GetComponentInParent<PlayerController>()) return;
        consumed = true;
        Destroy(gameObject);
    }
}
