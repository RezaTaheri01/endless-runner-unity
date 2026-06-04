using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem; // Required for Unity's new Input System

public class PlayerMovement : MonoBehaviour
{
    #region Components
    [Header("Components")]
    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;
    private Animator anim;
    #endregion
    
    #region Input
    [Header("Input")]
    // Allows assigning in Unity Inspector without making the field public
    [SerializeField] private InputActionReference jumpAction; // Reference to the Jump input action asset
    // Reference to the Move input action asset
    [SerializeField]private InputActionReference moveAction; 
    private Vector2 moveInput; // Stores the current movement direction (-1, 0, or 1 for x-axis)
    #endregion

    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField]
    private float jumpForce = 10f; // How high the player jumps (upward force applied)
    [SerializeField]
    private float moveSpeed = 5f; // How fast the player moves horizontally
    #endregion

    #region Ground Detection
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    private bool isGrounded = true;
    #endregion

    #region Game State
    [Header("Game State")]
    private bool playerUnlocked = false;
    #endregion


    // Called when the script instance is being loaded (before Start)
    // As soon as object loads
    private void Awake() {
        // Get components attached to the same GameObject
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }


    // Start: called once before first Update, use for initialization
    // Before first Update, after script enables
    private void Start() {
        anim.SetBool("isGrounded", true);
        anim.SetFloat("xVelocity", 0f);
        anim.SetFloat("yVelocity", 0f);
    }


    // Called when the object becomes enabled and active
    private void OnEnable() {
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
    private void OnDisable() {
        // Unsubscribe from events to prevent memory leaks and null reference errors
        jumpAction.action.performed -= OnJump;
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
        
        // Disable the input actions to stop listening for input
        jumpAction.action.Disable();
        moveAction.action.Disable();
    }


    // Called when the Jump action is performed (space button or controller button pressed)
    private void OnJump(InputAction.CallbackContext context) {
        if (!playerUnlocked)
            playerUnlocked = true;

        if (!isGrounded)
            return;

        // Check if the action was actually performed (not just started or canceled)
        if (context.performed)
        {
            isGrounded = false;

            // Apply upward force to make the player jump
            // linearVelocityY sets the vertical velocity (positive = upward)
            rb.linearVelocityY = jumpForce;
            
            // Debug log to confirm jump is working
            Debug.Log("Jumping via manual reference!");
        }
    }


    // Called when Move action is performed (key pressed) or canceled (key released)
    private void OnMove(InputAction.CallbackContext context) {
        // Read the movement input value (Vector2: x = horizontal, y = vertical)
        // For 2D platformers, we typically only use x value for left/right movement
        moveInput = context.ReadValue<Vector2>();
    }


    // Called once per frame. If FPS changes, the frequency of Update calls changes.
    private void Update()
    {
        CheckCollision();
    }


    // Called every fixed framerate frame (used for physics calculations)
    // FixedUpdate is used instead of Update for consistent physics timing
    // Default is 50 per second
    private void FixedUpdate() {
        if (!playerUnlocked){
            Debug.Log("Unlock by press jump key!");
            return;
        }

        AnimatorController();

        // Apply horizontal movement velocity
        rb.linearVelocityX = moveInput.x * moveSpeed;
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

    private void CheckCollision() {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
    }


    // Flip face by changing transform.localScale.x
    private void AnimatorController() {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));         

        if (moveInput.x > 0){
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if(moveInput.x < 0){
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
    }

}

