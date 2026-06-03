using UnityEngine;
using UnityEngine.InputSystem; // Required for Unity's new Input System

public class PlayerMovement : MonoBehaviour
{
    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;
    
    [SerializeField] // Allows assigning in Unity Inspector without making the field public
    private InputActionReference jumpAction; // Reference to the Jump input action asset
    [SerializeField]
    private InputActionReference moveAction; // Reference to the Move input action asset
    
    [SerializeField]
    private float jumpForce = 10f; // How high the player jumps (upward force applied)
    [SerializeField]
    private float moveSpeed = 5f; // How fast the player moves horizontally
    
    private Vector2 moveInput; // Stores the current movement direction (-1, 0, or 1 for x-axis)

    // Called when the script instance is being loaded (before Start)
    private void Awake()
    {
        // Get the Rigidbody2D component attached to the same GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    // Called when the object becomes enabled and active
    private void OnEnable()
    {
        // Enable the input actions so they start listening for input
        jumpAction.action.Enable();
        moveAction.action.Enable();
        
        // Subscribe to the performed event (triggered when input action is activated)
        jumpAction.action.performed += OnJump; // Jump when space/button is pressed
        
        // Move when movement input is performed (key pressed) or canceled (key released)
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove; // Also handle when input stops (key released)
    }

    // Called when the object becomes disabled or inactive
    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks and null reference errors
        jumpAction.action.performed -= OnJump;
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
        
        // Disable the input actions to stop listening for input
        jumpAction.action.Disable();
        moveAction.action.Disable();
    }

    // Called when the Jump action is performed (space button or controller button pressed)
    private void OnJump(InputAction.CallbackContext context)
    {
        // Check if the action was actually performed (not just started or canceled)
        if (context.performed)
        {
            // Apply upward force to make the player jump
            // linearVelocityY sets the vertical velocity (positive = upward)
            rb.linearVelocityY = jumpForce;
            
            // Debug log to confirm jump is working
            Debug.Log("Jumping via manual reference!");
        }
    }

    // Called when Move action is performed (key pressed) or canceled (key released)
    private void OnMove(InputAction.CallbackContext context)
    {
        // Read the movement input value (Vector2: x = horizontal, y = vertical)
        // For 2D platformers, we typically only use x value for left/right movement
        moveInput = context.ReadValue<Vector2>();
    }

    // Called every fixed framerate frame (used for physics calculations)
    // FixedUpdate is used instead of Update for consistent physics timing
    private void FixedUpdate()
    {
        // Flip face by changing transform.localScale.x
        if (moveInput.x > 0)
        {
            // Moving right - face right
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }else if(moveInput.x < 0){
            // Moving left - face left
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        // Apply horizontal movement velocity
        // x = horizontal input (-1 for left, 1 for right) multiplied by move speed
        // Y velocity remains unchanged (gravity affects it automatically)
        rb.linearVelocityX = moveInput.x * moveSpeed;
    }
}