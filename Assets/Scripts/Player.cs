using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const float MAX_FLOOR_ANGLE_EPSILON = 0.1F;

    private bool _jumpWasRequested;
    private bool _isOnFloor;
    private InputAction _walkAction;
    private InputAction _jumpAction;
    private BoxCollider2D _collider;
    private Rigidbody2D _body;

    public float maxFloorAngleDegrees = 45.0F;

    public void Awake()
    {
        maxFloorAngleDegrees += MAX_FLOOR_ANGLE_EPSILON;
    }

    public void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _body = GetComponent<Rigidbody2D>();
        _walkAction = InputSystem.actions.FindAction("Walk");
        _jumpAction = InputSystem.actions.FindAction("Jump");

        _jumpAction.performed += _ =>
        {
            _jumpWasRequested = true;
        };
    }

    public void FixedUpdate()
    {
        // might have been set somewhere between fixed updates
        if (!_isOnFloor)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(_collider.transform.position, _collider.size, 0, Vector2.down, 0.01F);

            foreach (var hit in hits)
            {
                if (_isOnFloor)
                {
                    break;
                }

                _isOnFloor |= hit.collider != _collider && Mathf.Acos(Vector2.Dot(hit.normal, Vector2.up)) <= maxFloorAngleDegrees;
            }
        }


        var linearVelocity = _body.linearVelocity;

        if (_jumpWasRequested && _isOnFloor)
        {
            linearVelocity.y += 13;
        }

        linearVelocity.y -= 30 * Time.fixedDeltaTime;

        var walkValue = _walkAction.ReadValue<float>() * 200 * Time.fixedDeltaTime;

        linearVelocity.x = walkValue;
        _body.linearVelocity = linearVelocity;

        _isOnFloor = false;
        _jumpWasRequested = false;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D c in collision.contacts)
        {
            _isOnFloor |= Mathf.Acos(Vector2.Dot(c.normal, Vector2.up)) <= maxFloorAngleDegrees;

            if (_isOnFloor)
            {
                break;
            }
        }
    }
}
