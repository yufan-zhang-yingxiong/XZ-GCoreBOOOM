using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class SquareController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D Rb;
    public LineRenderer Lr;
    public DistanceJoint2D Dj;
    public CircleController Cc;

    [Header("Checks")]
    public LayerMask groundLayer;

    [Header("Move")]
    public float movementSpeed = 10f;
    public float acceleration = 7f;
    public float deceleration = 7f;
    public float velPower = .8f;
    public float frictionAmount = 1f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.5f;

    [Header("In-air Rotation")]
    public float addRotationRatio = 0.1f;
    public float addRotationLimit = 10f;
    public float rotationHardLimit = 200f;

    [Header("Connect")]
    public float connectForce = 10f;
    public float circleSetMass = 10f;
    public float circleSetMassDuration = 0.3f;

    public PlayerInputActions playerInputAction;

    private InputAction move;
    private InputAction jump;
    private InputAction connect;

    private BoxCollider2D _collider;
    private BoxCollider2D Collider
    {
        get
        {
            if (_collider == null)
                _collider = GetComponent<BoxCollider2D>();
            if (_collider == null)
                Debug.LogError("Box Collider 2D is not found!");
            return _collider;
        }
    }

    private Vector2 InputMovementDirection { get { return move.ReadValue<Vector2>(); } }
    private float _lastGroundedTime;
    private float _jumBufferTime;
    private bool isGrounded
    {
        get
        {
            if (GroundNormal.magnitude > 0.01f && Vector2.Angle(-GroundNormal, -Vector2.up) < 80f)
            {
                return true;
            }
            return false;
        }
    }

    private Vector2 GroundNormal
    {
        get
        {
            ContactPoint2D[] contactsPts = new ContactPoint2D[10];
            int count = Collider.GetContacts(contactsPts);
            if (count == 0) return Vector2.zero;
            Vector2 _normal = Vector2.zero;
            foreach (var pt in contactsPts)
            {
                if (pt.collider == null) continue;
                if (pt.point.y < transform.position.y) _normal += pt.normal;
            }
            return _normal.normalized;
        }
    }

    private bool isJumping { get { return !isGrounded && Rb.velocity.y > 0.01f; } }
    private bool isFalling { get { return !isGrounded && Rb.velocity.y < -0.01f; } }
    private bool _isConnected;
    private bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }
    
    private void OnEnable()
    {
        move = playerInputAction.Player.Move;
        move.Enable();

        jump = playerInputAction.Player.Jump;
        jump.Enable();
        jump.performed += Jump;

        connect = playerInputAction.Player.Connect;
        connect.Enable();
        connect.performed += Connect;
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
    }

    private void Awake()
    {
        if (Rb == null)
            Rb = GetComponent<Rigidbody2D>();
        playerInputAction = new PlayerInputActions();
        _lastGroundedTime = 0f;
        _jumBufferTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        _lastGroundedTime -= Time.deltaTime;
        _jumBufferTime -= Time.deltaTime;
        
        if (isGrounded) _lastGroundedTime = coyoteTime;
    }

    private void FixedUpdate()
    {
        float targetSpeed = movementSpeed * InputMovementDirection.x;
        float speedDif = targetSpeed - Rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        Vector2 dir = Vector2.right;
        if (isGrounded) dir = Vector3.Cross(Vector3.back, GroundNormal);
        Rb.AddForce(movement * dir);

        if (!isGrounded)
        {
            if (Mathf.Abs(Rb.angularVelocity) < addRotationLimit)
                Rb.AddTorque(-InputMovementDirection.x * addRotationRatio);
        }

        if (_jumBufferTime > 0f) _Jump();

        if (Mathf.Abs(Rb.angularVelocity) > rotationHardLimit)
        {
            Rb.angularVelocity = rotationHardLimit * Mathf.Sign(Rb.angularVelocity);
        }

        // Friction
        if (_lastGroundedTime > 0 && Mathf.Abs(InputMovementDirection.x) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(Rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(Rb.velocity.x);
            Rb.AddForce(dir * -amount, ForceMode2D.Impulse);
        }

        BetterGravity(Rb);
    }

    private void Jump(InputAction.CallbackContext contxt)
    {
        if (isJumping) return;
        _jumBufferTime = jumpBufferTime;
    }
    private void _Jump()
    {
        if (!isGrounded && _lastGroundedTime < 0) return;
        _jumBufferTime = 0f;
        _lastGroundedTime = 0f;
        Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Connect(InputAction.CallbackContext contxt)
    {
        if (!IsConnected)
        {
            Vector2 dir = Dj.transform.position - transform.position;
            Rb.AddForce(dir * connectForce, ForceMode2D.Impulse);
            Cc.SetMass(circleSetMass, circleSetMassDuration);
        }
        IsConnected = !IsConnected;
        Dj.enabled = IsConnected;
        Lr.enabled = IsConnected;
    }
    private void BetterGravity(Rigidbody2D rb)
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !jump.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
