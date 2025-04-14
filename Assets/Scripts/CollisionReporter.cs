using UnityEngine;
using UnityEngine.Events;

public class CollisionReporter : MonoBehaviour
{
    public readonly UnityEvent<Collision2D> CollisionEntered2D = new();
    public readonly UnityEvent<Collision2D> CollisionExited2D = new();
    public readonly UnityEvent<Collider2D> TriggerEntered2D = new();
    public readonly UnityEvent<Collider2D> TriggerExited2D = new();

    public void OnCollisionEnter2D(Collision2D other)
    {
        CollisionEntered2D.Invoke(other);
    }

    public void OnCollisionExit2D(Collision2D other)
    {
        CollisionExited2D.Invoke(other);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        TriggerEntered2D.Invoke(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        TriggerExited2D.Invoke(other);
    }
}