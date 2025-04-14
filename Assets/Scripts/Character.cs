using UnityEngine;

public struct Character
{
    public const string LAYER_MASK_NAME = "Player";

    public Rigidbody2D Body;
    public Collider2D Collider;

    public Character(GameObject gameObject)
    {
        Body = gameObject.GetComponent<Rigidbody2D>();
        Collider = gameObject.GetComponent<Collider2D>();
        gameObject.layer = LayerMask.GetMask();
        gameObject.AddComponent<CollisionReporter>();
    }
}