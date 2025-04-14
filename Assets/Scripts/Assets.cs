using UnityEngine;

public class Assets : MonoBehaviour
{
    private static bool _wasInitializedAlready;
    public static AnimationClip PlayerIdleAnimation { get; private set; }
    public static AnimationClip PlayerWalkAnimation { get; private set; }
    public static AudioClip Whip0AudioClip { get; private set; }

    public void Awake()
    {
        if (_wasInitializedAlready)
        {
            Destroy(gameObject);
            return;
        }

        _wasInitializedAlready = true;
        CacheAll();
    }

    private void CacheAll()
    {
        PlayerIdleAnimation = Resources.Load<AnimationClip>("Animations/Player/Idle");
        PlayerWalkAnimation = Resources.Load<AnimationClip>("Animations/Player/Walk");
        Whip0AudioClip = Resources.Load<AudioClip>("Audio/Sounds/10");
    }
}
