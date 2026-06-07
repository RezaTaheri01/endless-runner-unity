using System.Collections;
using System.ComponentModel;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem; // Required for Unity's new Input System

public class PlayerMovement : MonoBehaviour
{
    #region Move Info
    [Header("Move Info")]
    [SerializeField] private float moveSpeed = 5f; // How fast the player moves horizontally
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float speedMultiplier = 1.1f;
    private float defaultSpeed;
    private float defaultSlideSpeed;
    [Space]
    [SerializeField] private float milestoneIncreaser;
    private float defaultMilestoneIncreaser;
    private float speedMilestone;

    #endregion

    #region Components
    [Header("Components")]

    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    #endregion

    #region Input
    [Header("Input")]
    // Allows assigning in Unity Inspector without making the field public
    [SerializeField] private InputActionReference jumpAction; // Reference to the Jump input action asset
    // Reference to the Move input action asset
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference slideAction;
    private Vector2 moveInput; // Stores the current movement direction (-1, 0, or 1 for x-axis)
    #endregion

    #region Jump Settings
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f; // How high the player jumps (upward force applied)
    [SerializeField] private float doubleJumpForce = 10f;
    private bool canDoubleJump;
    #endregion

    #region Ground/Platform Detection
    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded = true;
    #endregion

    #region Wall Detection
    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    [SerializeField] private LayerMask wallLayer;
    private bool isNearWall = false;
    #endregion

    #region Game State
    [Header("Game State")]

    [SerializeField] private bool DEBUG_MODE = false;
    private bool isDead;
    private bool movementDisabled;
    private bool playerUnlocked = false;
    #endregion

    #region Slide Info
    [Header("Slide Info")]
    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float slideTimer;
    [SerializeField] private float slideCooldownTimer;
    [SerializeField] private float ceilingCheckDistance = 0.2f;
    [SerializeField] private LayerMask ceilingLayer;
    private float slideTimerCounter;
    private float slideCooldownTimerCounter;
    private bool isSliding = false;
    private bool canSliding = true;
    private bool ceilingDetected = false;
    #endregion

    # region Ledge Info
    [Header("Ledge Info")]
    [HideInInspector] public bool ledgeDetected;

    [SerializeField] private Vector2 climbBegunOffset;
    [SerializeField] private Vector2 climbOverOffset;

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;
    [SerializeField] private bool isClimbing;

    # endregion

    # region Knockback Info
    [Header("Knockback Info")]
    [SerializeField] private Vector2 knockbackDir;
    private bool isKnocked;
    private bool canKnocked = true;
    # endregion

    // Called when the script instance is being loaded (before Start)
    // As soon as object loads
    private void Awake()
    {
        // Get components attached to the same GameObject
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }


    // Start: called once before first Update, use for initialization
    // Before first Update, after script enables
    private void Start()
    {
        Debug.Log("Unlock by press jump key!");

        anim.SetBool("isGrounded", true);
        anim.SetFloat("xVelocity", 0f);
        anim.SetFloat("yVelocity", 0f);

        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultSlideSpeed = slideSpeed;
        defaultMilestoneIncreaser = milestoneIncreaser;
    }


    # region Input Manager
    // Called when the object becomes enabled and active
    private void OnEnable()
    {
        // Enable the input actions so they start listening for input
        jumpAction.action.Enable();
        slideAction.action.Enable();
        moveAction.action.Enable();

        // Subscribe to the performed event (triggered when input action is activated)
        jumpAction.action.performed += OnJump; // Jump when space/button is pressed

        // // Move when movement input is performed (key pressed) or canceled (key released)
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove; // Also handle when input stops (key released)


        // Slide when slide input is performed
        slideAction.action.performed += OnSlide;
    }


    // Called when the object becomes disabled or inactive
    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks and null reference errors
        jumpAction.action.performed -= OnJump;
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
        slideAction.action.performed -= OnSlide;

        // Disable the input actions to stop listening for input
        jumpAction.action.Disable();
        moveAction.action.Disable();
        slideAction.action.Disable();
    }


    // Called when the Jump action is performed (space button or controller button pressed)
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!playerUnlocked)
        {
            playerUnlocked = true;
            return;
        }

        if (movementDisabled)
            return;

        if (isSliding && ceilingDetected)
        {
            return; // Prevent jumping if there's a ceiling above
        }

        if (!isGrounded)
            if (canDoubleJump)
            {
                anim.SetBool("canRoll", false);
                canDoubleJump = false;
                rb.linearVelocityY = doubleJumpForce;
                return;
            }
            else
                return;

        // Check if the action was actually performed (not just started or canceled)
        if (context.performed)
        {
            // Todo - Stop Sliding if player can jump not if player in narrow place
            if (isSliding)
            {
                isSliding = false; // Stop sliding when jumping
                slideCooldownTimerCounter = slideCooldownTimer; // Reset slide cooldown when jumping
            }
            else
            {
                slideCooldownTimerCounter = 0; // Allow sliding immediately if not sliding
            }

            canSliding = false; // Prevent sliding immediately after jumping
            isGrounded = false;

            // Apply upward force to make the player jump
            // linearVelocityY sets the vertical velocity (positive = upward)
            rb.linearVelocityY = jumpForce;
        }
    }


    // // Called when Move action is performed (key pressed) or canceled (key released)
    private void OnMove(InputAction.CallbackContext context)
    {
        if (movementDisabled)
        {
            moveInput = Vector2.zero;
            return;
        }

        // Read the movement input value (Vector2: x = horizontal, y = vertical)
        // For 2D platformers, we typically only use x value for left/right movement
        moveInput = context.ReadValue<Vector2>();
    }


    private void OnSlide(InputAction.CallbackContext context)
    {
        if (movementDisabled)
            return;

        if (isGrounded && !isSliding && canSliding)
        {
            isSliding = true;
            slideTimerCounter = slideTimer;
        }
    }

    #endregion


    # region Update
    // Called once per frame. If FPS changes, the frequency of Update calls changes.
    private void Update()
    {
        AnimatorController();
        SlidingCheck();
    }


    // Called every fixed framerate frame (used for physics calculations)
    // FixedUpdate is used instead of Update for consistent physics timing
    // Default is 50 per second
    private void FixedUpdate()
    {
        if (isKnocked || movementDisabled || isDead || !playerUnlocked) return;

        CheckCollision();

        LedgeClimbCheck();
        SpeedController();

        // Apply horizontal movement velocity
        if (isSliding)
        {
            rb.linearVelocityX = slideSpeed;
        }
        else
        {
            if (DEBUG_MODE)
                rb.linearVelocityX = moveInput.x * moveSpeed;
            else
                rb.linearVelocityX = moveSpeed;
        }
    }
    #endregion

    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        ceilingDetected = Physics2D.Raycast(transform.position, Vector2.up, ceilingCheckDistance, ceilingLayer);
        isNearWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

        if (isGrounded)
        {
            canDoubleJump = true; // Reset double jump when grounded
        }

        if (isNearWall)
        {
            SpeedReset();
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

        Gizmos.color = ceilingDetected ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * ceilingCheckDistance);

        Gizmos.color = isNearWall ? Color.green : Color.red;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }


    private void AnimatorController()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("canClimb", isClimbing);
        anim.SetBool("isKnocked", isKnocked);

        if (rb.linearVelocity.y < -25)
        {
            anim.SetBool("canRoll", true);
        }
    }


    private void RollAnimFinished()
    {
        anim.SetBool("canRoll", false);
    }


    private void SlidingCheck()
    {
        if (isSliding)
        {
            slideTimerCounter = Mathf.Max(0, slideTimerCounter - Time.deltaTime);

            if (slideTimerCounter <= 0 && !ceilingDetected)
            {
                isSliding = false;
                canSliding = false;
                slideCooldownTimerCounter = slideCooldownTimer;
                slideTimerCounter = slideTimer;
            }
        }

        if (!canSliding)
        {
            slideCooldownTimerCounter = Mathf.Max(0, slideCooldownTimerCounter - Time.deltaTime);
            if (slideCooldownTimerCounter <= 0)
            {
                canSliding = true;
            }
        }
    }


    # region Ledge Climb
    private void LedgeClimbCheck()
    {
        if (ledgeDetected && canGrabLedge && !isClimbing)
        {
            movementDisabled = true;
            canGrabLedge = false;
            isClimbing = true;

            Vector2 ledgePosition = GetComponentInChildren<LedgeCheck>().transform.position;

            climbBegunPosition = ledgePosition + climbBegunOffset;
            climbOverPosition = ledgePosition + climbOverOffset;

            rb.bodyType = RigidbodyType2D.Static;
            transform.position = climbBegunPosition;
        }
    }


    private void LedgeClimbOver()
    {
        movementDisabled = false;
        isClimbing = false;
        isGrounded = true;
        transform.position = climbOverPosition;
        rb.bodyType = RigidbodyType2D.Dynamic;

        anim.SetBool("isGrounded", isGrounded);

        // Call AllowGrabLedge with delay
        Invoke("AllowGrabLedge", 1f);
    }

    private void AllowGrabLedge() => canGrabLedge = true;
    # endregion

    private void SpeedController()
    {
        if (moveSpeed == maxSpeed) return;

        // Moving right: x is always increasing
        if (transform.position.x > speedMilestone)
        {
            Debug.Log("Player reach milestone, Speed increasing");

            // Increase Move Speed
            moveSpeed = Mathf.Min(maxSpeed, moveSpeed * speedMultiplier);
            slideSpeed = Mathf.Min(maxSpeed, slideSpeed * speedMultiplier);

            // Define next milestone
            speedMilestone = transform.position.x + milestoneIncreaser;
            milestoneIncreaser *= speedMultiplier;
        }
    }

    private void SpeedReset()
    {
        moveSpeed = defaultSpeed;
        slideSpeed = defaultSlideSpeed;
        milestoneIncreaser = defaultMilestoneIncreaser;
        speedMilestone = transform.position.x + milestoneIncreaser;
    }

    private void Knockback()
    {
        if (!canKnocked) return;

        isKnocked = true;
        rb.linearVelocity = knockbackDir;
        StartCoroutine(invincibility());
    }

    private void CancelKnockback() => isKnocked = false;

    private IEnumerator invincibility()
    {
        Color originalColor = sr.color;
        Color darkerColor = new Color(sr.color.r, sr.color.g, sr.color.b, .5f);

        canKnocked = false;

        for (int i = 0; i < 12; i++)
        {
            sr.color = darkerColor;
            yield return new WaitForSeconds(.2f);
            sr.color = originalColor;
            yield return new WaitForSeconds(.2f);
        }

        canKnocked = true;
    }


    private IEnumerator Die()
    {
        movementDisabled = true;
        isDead = true;
        anim.SetBool("isDead", true);
        yield return new WaitForSeconds(.5f);
        rb.linearVelocity = Vector2.zero;
    }
}

