using UnityEngine;

public class MedusaHead : MonoBehaviour
{
    public float MoveSpeed = 2;
    public float AmplitudeFactor = 2;
    public float PeriodFactor = 200;

    public void FixedUpdate()
    {
        var newPos = transform.localPosition;

        newPos.x -= MoveSpeed * Time.fixedDeltaTime;
        newPos.y = AmplitudeFactor * Mathf.Sin(Time.time * Time.fixedDeltaTime * PeriodFactor) - AmplitudeFactor / 2.0F;

        transform.localPosition = newPos;
    }
}
