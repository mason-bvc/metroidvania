using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform Target;
    public float FollowSpeed = 1;

    public bool FollowX = true;
    public bool FollowY = true;
    public bool FollowZ = true;

    public void Update()
    {
        var newPosition = transform.position;

        if (FollowX)
        {
            newPosition.x = Target.position.x;
        }

        if (FollowY)
        {
            newPosition.y = Target.position.y;
        }

        if (FollowZ)
        {
            newPosition.z = Target.position.z;
        }

        if (FollowSpeed > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.position, FollowSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position = newPosition;
        }
    }
}
