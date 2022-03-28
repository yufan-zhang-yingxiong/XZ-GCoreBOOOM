using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class SquareController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D Rb;

    [Header("Checks")]
    [SerializeField]
    private Transform[] groundCheckers;
    public LayerMask groundLayer;

    [Header("Move")]
    public float movementSpeed = 10f;
    public float acceleration = 7f;
    public float decceleration = 7f;
    public float velPower = .8f;
    public float frictionAmount = 1f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.5f;

    public PlayerInputActions playerInputAction;

    private InputAction move;
    private InputAction jump;

    private BoxCollider2D _collider;
    private BoxCollider2D Collider { 
        get { 
            if (_collider == null)
                _collider = GetComponent<BoxCollider2D>();
            if (_collider == null)
                Debug.LogError("Box Collider 2D is not found!");
            return _collider;
        } 
    }

    private Vector2 movementDirection { get { return move.ReadValue<Vector2>(); } }
    private float _lastGroundedTime;
    private bool isGrounded { get {
            bool result = false;
            ContactPoint2D[] contactsPts = new ContactPoint2D[10];
            int count = Collider.GetContacts(contactsPts);
            if (count == 0) return false;

            Vector2 from = new Vector2();
            foreach (var pt in contactsPts)
            {
                if (pt.collider == null) continue;
                from += -pt.normal;
            }
            if (Vector2.Angle(from, -Vector2.up) < 80f) result = true;
            return result;
        } 
    }
    private bool isJumping { get { return Rb.velocity.y > 0; } }
    

    private void OnEnable()
    {
        move = playerInputAction.Player.Move;
        move.Enable();

        jump = playerInputAction.Player.Jump;
        jump.Enable();
        jump.performed += Jump;
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
    }

    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        _lastGroundedTime -= Time.deltaTime;
        if (isGrounded && !isJumping) _lastGroundedTime = coyoteTime;
    }

    private void FixedUpdate()
    {

        float targetSpeed = movementSpeed * movementDirection.x;
        float speedDif = targetSpeed - Rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        Rb.AddForce(movement * Vector2.right);

        // Friction
        if (_lastGroundedTime > 0 && Mathf.Abs(movementDirection.x) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(Rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(Rb.velocity.x);
            Rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        BetterGravity(Rb);
    }

    private void Jump(InputAction.CallbackContext contxt)
    {
        if (!isGrounded && _lastGroundedTime < 0) return;
        _lastGroundedTime = 0f;
        Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void BetterGravity(Rigidbody2D rb)
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        } else  if (rb.velocity.y > 0 && !jump.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmos()
    {
    }
}
