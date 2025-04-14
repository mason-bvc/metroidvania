using UnityEngine;

public class Ghoul : MonoBehaviour
{
    private Character _character;

    public float WalkSpeed = 200;

    public void Awake()
    {
        _character = new(gameObject);
    }

    public void FixedUpdate()
    {
        _character.Body.linearVelocityX = -WalkSpeed * Time.fixedDeltaTime;
    }
}
