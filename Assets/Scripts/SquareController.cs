using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class SquareController : MonoBehaviour
{
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

    [Header("Push-pull")]
    public float pushNPullSpeed = 10f;
    public float maxDistance = 4f;
    
    /*
    [Header("Connect")]
    public float connectForce = 10f;
    public float circleSetMass = 10f;
    public float circleSetMassDuration = 0.3f;
    */

    #region InputAction 
    public PlayerInputActions playerInputAction;

    private InputAction move;
    private InputAction jump;
    private InputAction connect;
    private InputAction push;
    private InputAction pull;
    private Vector2 InputMovementDirection { get { return move.ReadValue<Vector2>(); } }
    #endregion

    #region Colider2D Related
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
    #endregion

    #region Jump Variables
    private float _lastGroundedTime;
    private float _jumBufferTime;
    private bool _isConnected;

    public bool isGrounded
    {
        get
        {
            return GroundNormal.magnitude > 0.01f && Vector2.Angle(-GroundNormal, -Vector2.up) < 80f;
        }
    }

    public Vector2 GroundNormal
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

    public bool isJumping { get { return !isGrounded && Rb.velocity.y > 0.01f; } }
    public bool isFalling { get { return !isGrounded && Rb.velocity.y < -0.01f; } }
    public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }
    #endregion

    #region Util Variables
    public float distToCircle { get { return Vector2.Distance(transform.position, Circle.position); } }
    #endregion

    #region Reference
    private Rigidbody2D _Rb;
    public Rigidbody2D Rb
    {
        get
        {
            if (_Rb == null) _Rb = GetComponent<Rigidbody2D>();
            return _Rb;
        }
    }
    private LineRenderer _Lr;
    public LineRenderer Lr {
        get
        {
            if (_Lr == null) _Lr = GetComponentInChildren<LineRenderer>();
            return _Lr;
        }
    }
    private DistanceJoint2D _Dj;
    public DistanceJoint2D Dj
    {
        get
        {
            if (_Dj == null) _Dj = GetComponentInChildren<DistanceJoint2D>();
            return _Dj;
        }
    }
    private Transform _Circle;
    public Transform Circle
    {
        get
        {
            if (_Circle == null) _Circle = GameObject.FindWithTag("Circle").transform;
            return _Circle;
        }
    }
    #endregion

    private void OnEnable()
    {
        move = playerInputAction.Player.Move;
        jump = playerInputAction.Player.Jump;
        push = playerInputAction.Player.Push;
        pull = playerInputAction.Player.Pull;
        connect = playerInputAction.Player.Connect;

        move.Enable();
        jump.Enable();
        push.Enable();
        pull.Enable();
        connect.Enable();

        jump.performed += Jump;
        connect.performed += Connect;
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        connect.Disable();
    }

    private void Awake()
    {
        Dj.connectedBody = Circle.GetComponent<Rigidbody2D>();
        playerInputAction = new PlayerInputActions();

        _lastGroundedTime = 0f;
        _jumBufferTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Update timer
        _lastGroundedTime -= Time.deltaTime;
        _jumBufferTime -= Time.deltaTime;
        
        // Set grounded time to coyote time
        // So that the player will still be allow to jump even not on ground fo a tiny interval
        if (isGrounded) _lastGroundedTime = coyoteTime;
    }

    private void FixedUpdate()
    {
        float targetSpeed = movementSpeed * InputMovementDirection.x;
        float speedDif = targetSpeed - Rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        // The right-direction of player
        Vector2 dir = Vector2.right;
        
        // Set dir if the ground is not horizontal
        // in order to add backward force on the player
        if (isGrounded) dir = Vector3.Cross(Vector3.back, GroundNormal);
        Rb.AddForce(movement * dir);

        // Add a little rotation if the player is in the air to add some dynamics
        if (!isGrounded)
        {
            if (Mathf.Abs(Rb.angularVelocity) < addRotationLimit)
                Rb.AddTorque(-InputMovementDirection.x * addRotationRatio);
        }
        // Make sure the sqaure does not rotate like crazy
        if (Mathf.Abs(Rb.angularVelocity) > rotationHardLimit)
        {
            Rb.angularVelocity = rotationHardLimit * Mathf.Sign(Rb.angularVelocity);
        }

        // Sometimes player can't jump even visually really near the ground
        // Set a jump buffer will make sure that the player is still jumping after a tiny interval 
        if (_jumBufferTime > 0f) _Jump();

        // Friction
        if (_lastGroundedTime > 0 && Mathf.Abs(InputMovementDirection.x) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(Rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(Rb.velocity.x);
            Rb.AddForce(dir * -amount, ForceMode2D.Impulse);
        }

        // Update line length
        
        if (push.IsPressed())
        {
            
            Dj.distance -= pushNPullSpeed * Time.fixedDeltaTime;
        }
        if (pull.IsPressed())
        {
            if (Dj.distance < maxDistance)
                Dj.distance += pushNPullSpeed * Time.fixedDeltaTime;
            else
                Dj.distance = maxDistance;
        }

        // Add better gravity
        BetterGravity(Rb);
    }

    // Set jump buffer time, next physic frame will be checking if player is allowed to jump
    private void Jump(InputAction.CallbackContext contxt)
    {
        if (isJumping) return;
        _jumBufferTime = jumpBufferTime;
    }

    private void _Jump()
    {
        // Check if allow to jump
        if (!isGrounded && _lastGroundedTime < 0) return;
        // Clear timers
        _jumBufferTime = 0f;
        _lastGroundedTime = 0f;

        // Set velocity's vertical to 0
        // So jumping will be normal if player is moving down or up
        Vector2 v = Rb.velocity;
        v.y = 0;
        Rb.velocity = v;

        // Apply force to jump
        Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Connect(InputAction.CallbackContext contxt)
    {
        Dj.distance = distToCircle;
        if (distToCircle > maxDistance) Dj.distance = maxDistance;

        IsConnected = !IsConnected;
        Dj.enabled = IsConnected;
        Lr.enabled = IsConnected;
    }



    // By change gravity dynamicly, provide a better falling and jumping feeling
    private void BetterGravity(Rigidbody2D rb)
    {
        // If player is falling, make player fall faster than during jumping
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // The player will jump higher and fall slower if jump button is pressed
        else if (rb.velocity.y > 0 && !jump.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
