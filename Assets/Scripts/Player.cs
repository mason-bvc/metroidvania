using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const float MAX_FLOOR_ANGLE_EPSILON = 0.1F;
    private const float FLOOR_CHECK_NUDGE_DISTANCE = 0.01F;

    private bool _jumpWasRequested;
    private bool _isOnFloor;
    private bool _defaultFlipX;
    private float _currentWalkSpeed;
    private float _currentVerticalSpeed;

    private InputAction _walkAction;
    private InputAction _jumpAction;

    private BoxCollider2D _collider;
    private Rigidbody2D _body;

    private GameObject _visuals;
    private SpriteRenderer _visualsSpriteRenderer;
    private Animator _visualsAnimator;

    public float MaxFloorAngleDegrees = 45.0F;
    public float FallAcceleration = 40F;
    public float WalkSpeed = 200F;
    public float JumpStrength = 60F;

    public void Awake()
    {
        MaxFloorAngleDegrees += MAX_FLOOR_ANGLE_EPSILON;
    }

    public void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _body = GetComponent<Rigidbody2D>();

        _visuals = transform.Find("Visuals").gameObject;
        _visualsSpriteRenderer = _visuals.GetComponent<SpriteRenderer>();
        _visualsAnimator = _visuals.GetComponent<Animator>();

        _defaultFlipX = _visualsSpriteRenderer.flipX;
        _walkAction = InputSystem.actions.FindAction("Walk");
        _jumpAction = InputSystem.actions.FindAction("Jump");

        _walkAction.performed += ctx =>
        {
            _visualsSpriteRenderer.flipX = Convert.ToSingle(_defaultFlipX) * ctx.ReadValue<float>() > float.Epsilon;
        };

        _jumpAction.performed += _ =>
        {
            _jumpWasRequested = true;
        };
    }

    public void FixedUpdate()
    {
        // might have been set somewhere between fixed updates (OnCollisionEnter)
        if (!_isOnFloor)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                _collider.transform.position, _collider.size, 0, Vector2.down,
                FLOOR_CHECK_NUDGE_DISTANCE);

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

        // we want current vertical speed to be clipped and limited according
        // to the body's internal vertical speed
        // _currentVerticalSpeed = _body.linearVelocityY;

        _currentVerticalSpeed -= FallAcceleration;

        if (_jumpWasRequested && _isOnFloor)
        {
            _currentVerticalSpeed = JumpStrength;
        }

        if (_isOnFloor)
        {
            _currentWalkSpeed = _walkAction.ReadValue<float>();
            _currentWalkSpeed *= WalkSpeed;

            _body.linearVelocityX = _currentWalkSpeed * Time.fixedDeltaTime;
        }

        _body.linearVelocityY = _currentVerticalSpeed * Time.fixedDeltaTime;

        // the state only really changes as often as every physics tick, saving
        // some cylces by calling it in FixedUpdate hopefully
        UpdateAnimator();

        _isOnFloor = false;
        _jumpWasRequested = false;
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

    private string GetAnimationNameBasedOnState()
    {
        if (Mathf.Abs(_currentWalkSpeed) > float.Epsilon && _isOnFloor)
        {
            return "Walk";
        }

        if (Mathf.Abs(_currentVerticalSpeed) >= float.Epsilon)
        {
            if (_currentVerticalSpeed > 0)
            {
                return "Jump";
            }

            return "Fall";
        }

        return "Idle";
    }

    private void UpdateAnimator()
    {
        _visualsAnimator.Play(GetAnimationNameBasedOnState());
    }
}
