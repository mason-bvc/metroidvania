using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walking   = 1 << 0,
        Jumping   = 1 << 1,
        Falling   = 1 << 2,

        Attacking = 1 << 16,
    }

    private const float MAX_FLOOR_ANGLE_EPSILON = 0.1F;
    private const float FLOOR_CHECK_NUDGE_DISTANCE = 0.01F;

    private readonly Vector2 _floorCheckBoxColliderDimensions = new(1, 2);

    private bool _jumpWasRequested;
    private bool _attackWasRequested;
    private bool _isOnFloor;
    private bool _defaultFlipX;
    private float _currentWalkSpeed;
    private float _currentVerticalSpeed;
    private State _currentState;
    private float _currentStateCounter;

    private InputAction _walkAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;

    private Collider2D _collider;
    private Rigidbody2D _body;

    private GameObject _visuals;
    private SpriteRenderer _visualsSpriteRenderer;
    private Animator _visualsAnimator;
    private AudioSource _audioSource;

    public float MaxFloorAngleDegrees = 45.0F;
    public float FallAcceleration = 40F;
    public float WalkSpeed = 200F;
    public float JumpStrength = 60F;

    public int DefaultLayerMask { get; private set; }

    public static float GetStateDurationSeconds(State state) => state switch
    {
        State.Attacking => 0.3F,
        _ => 0,
    };

    // stub
    public static State GetNextStateFor(State _) => State.Idle;

    public void Awake()
    {
        DefaultLayerMask = LayerMask.GetMask("Default");
        MaxFloorAngleDegrees += MAX_FLOOR_ANGLE_EPSILON;
    }

    public void Start()
    {
        _collider = GetComponent<Collider2D>();
        _body = GetComponent<Rigidbody2D>();

        _visuals = transform.Find("Visuals").gameObject;
        _visualsSpriteRenderer = _visuals.GetComponent<SpriteRenderer>();
        _visualsAnimator = _visuals.GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _defaultFlipX = _visualsSpriteRenderer.flipX;
        _walkAction = InputSystem.actions.FindAction("Walk");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _attackAction = InputSystem.actions.FindAction("Attack");

        _walkAction.performed += ctx =>
        {
            if (_currentState != State.Attacking)
            {
                _visualsSpriteRenderer.flipX = Convert.ToSingle(_defaultFlipX) * ctx.ReadValue<float>() > float.Epsilon;
            }
        };

        _jumpAction.performed += _ =>
        {
            _jumpWasRequested = true;
        };

        _attackAction.performed += _ =>
        {
            _attackWasRequested = true;
        };
    }

    public void FixedUpdate()
    {
        DoMovement();
        UpdateState();
        // the state only really changes as often as every physics tick, saving
        // some cylces by calling it in FixedUpdate hopefully
        UpdateAnimator();
        ResetState();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D c in collision.contacts)
        {
            _isOnFloor |= Mathf.Acos(Vector2.Dot(c.normal, Vector2.up)) <= MaxFloorAngleDegrees;

            if (_isOnFloor)
            {
                break;
            }
        }
    }

    private void DoMovement()
    {
        UpdateFloorCheck();

        // we want current vertical speed to be clipped and limited according
        // to the body's internal vertical speed
        // _currentVerticalSpeed = _body.linearVelocityY;

        _currentVerticalSpeed -= FallAcceleration;

        if (_jumpWasRequested && _isOnFloor)
        {
            _currentVerticalSpeed = JumpStrength;
        }

        var canWalk = _isOnFloor && _currentState != State.Attacking;

        if (canWalk)
        {
            _currentWalkSpeed = _walkAction.ReadValue<float>();
            _currentWalkSpeed *= WalkSpeed;
        }
        else if (_isOnFloor)
        {
            _currentWalkSpeed = 0;
        }

        _body.linearVelocityX = _currentWalkSpeed * Time.fixedDeltaTime;
        _body.linearVelocityY = _currentVerticalSpeed * Time.fixedDeltaTime;
    }

    private void UpdateFloorCheck()
    {
        // might have been set somewhere between fixed updates (OnCollisionEnter)
        // also doing this check allows us to skip doing a possibly costly
        // box cast. hey, do you remember when they had BoxCastNonAlloc? what
        // ever happened to that?
        if (!_isOnFloor)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                _collider.transform.position,     // position
                _floorCheckBoxColliderDimensions, // size
                0,                                // angle
                Vector2.down,                     // direction
                FLOOR_CHECK_NUDGE_DISTANCE,       // distance
                DefaultLayerMask                  // layerMask
            );

            foreach (var hit in hits)
            {
                if (_isOnFloor)
                {
                    break;
                }

                _isOnFloor |=
                    hit.collider != _collider
                    && Mathf.Acos(Vector2.Dot(hit.normal, Vector2.up)) <= MaxFloorAngleDegrees;
            }
        }
    }

    private void UpdateState()
    {
        var newState = GetStateBasedOnState();

        if (newState != _currentState)
        {
            _currentState = newState;
            _currentStateCounter = GetStateDurationSeconds(_currentState);
        }

        _currentStateCounter -= Time.fixedDeltaTime;

        if (_currentStateCounter <= 0)
        {
            _currentStateCounter = 0;

            if (_currentState == State.Attacking)
            {
                _currentState = State.Idle;
            }
        }
    }

    // great method name right
    private State GetStateBasedOnState()
    {
        var shouldPlayAttackState = _attackWasRequested;

        shouldPlayAttackState |= _currentState == State.Attacking && _currentStateCounter > 0;

        if (shouldPlayAttackState)
        {
            _audioSource.clip = Assets.Whip0AudioClip;
            _audioSource.Play();

            return State.Attacking;
        }

        if (Mathf.Abs(_currentWalkSpeed) > float.Epsilon && _isOnFloor)
        {
            return State.Walking;
        }

        if (Mathf.Abs(_currentVerticalSpeed) >= float.Epsilon)
        {
            if (_currentVerticalSpeed > 0)
            {
                return State.Jumping;
            }

            return State.Falling;
        }

        return State.Idle;
    }

    private void ResetState()
    {
        _isOnFloor = false;
        _jumpWasRequested = false;

        if (_attackWasRequested && _currentStateCounter > 0)
        {
            _attackWasRequested = false;
        }
    }

    private string GetAnimationNameBasedOnState(State state) => state switch
    {
        State.Walking => "Walk",
        State.Jumping => "Jump",
        State.Falling => "Fall",
        State.Attacking => "Attack",
        _ => "Idle",
    };

    private void UpdateAnimator()
    {
        _visualsAnimator.Play(GetAnimationNameBasedOnState(_currentState));
    }
}
