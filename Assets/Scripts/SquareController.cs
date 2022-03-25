using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class SquareController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D Rb;

    public float movementSpeed = 10f;
    public float jumpForce = 10f;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    public PlayerInputActions playerInput;

    private InputAction move;
    private InputAction jump;

    private void OnEnable()
    {
        move = playerInput.Player.Move;
        move.Enable();

        jump = playerInput.Player.Jump;
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
        playerInput = new PlayerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 v = Rb.velocity;
        Vector2 moveDir = move.ReadValue<Vector2>();
        v.x = moveDir.x * movementSpeed;
        Rb.velocity = v;
        BetterJump(Rb);
    }

    private void Jump(InputAction.CallbackContext contxt)
    {
        Vector2 v = Rb.velocity;
        v.y = 0;
        Rb.velocity = v;
        Rb.AddForce(Vector2.up * jumpForce);
    }

    private void BetterJump(Rigidbody2D rb)
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        } else  if (rb.velocity.y > 0 && !jump.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
