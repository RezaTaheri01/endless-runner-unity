using System;
using System.Collections;
using System.ComponentModel;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private PlayerInputActions.PlayerControls inputActions;
    private Vector2 moveInput; // Stores the current movement direction (-1, 0, or 1 for x-axis)
    #endregion

    #region Jump Settings
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f; // How high the player jumps (upward force applied)
    [SerializeField] private float rollVelocityThreshold = -25f;
    private float doubleJumpForce = 10f;
    private bool canDoubleJump = false;
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
    [HideInInspector] public bool playerUnlocked;

    [HideInInspector] public bool extraLife = true;
    [HideInInspector] public int extraLifeCount = 1;
    private bool isRecharging = false;
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
    private bool isClimbing;

    # endregion

    # region Knockback Info
    [Header("Knockback Info")]
    [SerializeField] private Vector2 knockbackDir;
    [SerializeField] private Vector2 deathBackDir;
    private bool isKnocked;
    private bool canKnocked = true;
    # endregion

    # region Swipe Settings 
    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float swipeThresholdTime = 0.5f;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float touchStartTime;
    private bool isTouching = false;
    #endregion

    #region Dash Settings 
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashTimer;
    [SerializeField] private float dashCooldownTimer;
    [SerializeField] private Vector2 diamondHitForce = new Vector2(10, 15); // Change player velocity after hitting diamond
    private float dashTimerCounter;
    private float dashCooldownTimerCounter;
    private bool isDashing = false;
    private bool canDash = true;
    private bool hitGroundAfterDash = true;
    # endregion


    // Called when the script instance is being loaded (before Start)
    // As soon as object loads
    private void Awake()
    {
        inputActions = new PlayerInputActions.PlayerControls();

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


    #region Touch Manager
    private void DetectSwipe()
    {
        float swipeTime = Time.time - touchStartTime;

        // Check if swipe was fast enough
        if (swipeTime > swipeThresholdTime) return;

        Vector2 swipeDelta = touchEndPos - touchStartPos;
        float swipeDistance = swipeDelta.magnitude;

        // Check if swipe was long enough
        if (swipeDistance < minSwipeDistance) return;

        // Determine swipe direction (vertical)
        if (Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x))
        {
            // Vertical swipe
            if (swipeDelta.y > 0)
            {
                OnSwipeUp();
            }
            else
            {
                OnSwipeDown();
            }
        }
        else
        {
            // Horizontal swipe
            if (swipeDelta.x > 0)
            {
                OnSwipeRight();
            }
        }
    }

    private void OnSwipeUp()
    {
        Debug.Log("Swipe UP detected!");
        OnJumpHelper();
    }

    private void OnSwipeDown()
    {
        Debug.Log("Swipe DOWN detected!");
        OnSlideHelper();
    }


    private void OnSwipeRight()
    {
        Debug.Log("Swipe RIGHT detected!");
        OnDashHelper();
    }

    private void TrackTouchPosition()
    {
        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.isPressed)
        {
            if (!isTouching)
            {
                // Touch started
                isTouching = true;
                touchStartPos = touch.position.ReadValue();
                touchStartTime = Time.time;
            }
            else
            {
                // Touch continues - update end position
                touchEndPos = touch.position.ReadValue();
            }
        }
        else if (isTouching)
        {
            // Touch ended - process swipe
            isTouching = false;
            DetectSwipe();
        }
    }

    #endregion


    #region Input Manager
    // Called when the object becomes enabled and active
    private void OnEnable()
    {
        // Enable the input actions so they start listening for input
        inputActions.Enable();

        // Subscribe to the performed event (triggered when input action is activated)
        // Jump when space/button is pressed
        inputActions.Player.Jump.performed += OnJump;

        // Slide when slide input is performed
        inputActions.Player.Slide.performed += OnSlide;

        inputActions.Player.PauseGame.performed += OnPauseGame;

        inputActions.Player.Dash.performed += OnDash;

    }


    // Called when the object becomes disabled or inactive
    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks and null reference errors
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Slide.performed -= OnSlide;
        inputActions.Player.PauseGame.performed -= OnPauseGame;
        inputActions.Player.Dash.performed -= OnDash;

        // Disable the input actions to stop listening for input
        inputActions.Disable();
    }


    private void OnPauseGame(InputAction.CallbackContext context)
    {
        if (!playerUnlocked) return;

        UI_Main.instance.PauseGameButton();

        if (Time.timeScale == 1)
        {
            UI_Main.instance.SwitchMenuTo(UI_Main.instance.gameMenu);
        }
        else
        {
            UI_Main.instance.SwitchMenuTo(UI_Main.instance.pauseMenu);
        }
    }


    // Called when the Jump action is performed (space button or controller button pressed)
    private void OnJump(InputAction.CallbackContext context)
    {
        // if (!context.performed)
        //     return;

        OnJumpHelper();
    }


    private void OnJumpHelper()
    {
        if (movementDisabled)
            return;

        if (!playerUnlocked)
        {
            UI_Main.instance.SwitchMenuTo(UI_Main.instance.gameMenu);
            GameManager.instance.UnlockPlayer();
            return;
        }

        if (isSliding && ceilingDetected)
            return;

        if (!isGrounded)
        {
            //     if (!canDoubleJump)
            //         return;

            //     DoubleJump();
            return;
        }

        if (ceilingDetected) return;

        Jump();
    }


    private void OnSlide(InputAction.CallbackContext context)
    {
        // if (!context.performed)
        //     return;

        OnSlideHelper();
    }


    private void OnSlideHelper()
    {
        if (!playerUnlocked) return;

        if (movementDisabled)
            return;

        if (isGrounded && !isSliding && canSliding)
        {
            isSliding = true;
            slideTimerCounter = slideTimer;
        }
    }


    private void OnDash(InputAction.CallbackContext context)
    {
        OnDashHelper();
    }


    private void OnDashHelper()
    {
        if (!playerUnlocked)
            return;

        if (movementDisabled)
            return;

        if (ceilingDetected)
            return;


        if (!isDashing && canDash && !isGrounded && hitGroundAfterDash)
        {
            sr.color = GameManager.instance.playerDashColor;
            hitGroundAfterDash = false;
            isDashing = true;
            dashTimerCounter = dashTimer;
        }
    }

    #endregion


    # region Update
    // Called once per frame. If FPS changes, the frequency of Update calls changes.
    private void Update()
    {
        TrackTouchPosition();
        AnimatorController();
        SlidingCheck();
        DashingCheck();
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        if (Time.timeScale == 0)
            movementDisabled = true;
        else if (!isDead)
            movementDisabled = false;

        if (Mathf.Abs(rb.linearVelocityY) > 0.1f && isSliding)
        {
            isSliding = false;
        }
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
        if (isDashing)
        {
            SetHorizontalVelocity(dashSpeed);
        }
        else if (isSliding)
        {
            SetHorizontalVelocity(slideSpeed);
        }
        else
        {
            float direction = DEBUG_MODE ? moveInput.x : 1f;
            SetHorizontalVelocity(direction * moveSpeed);
        }
    }
    #endregion

    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        ceilingDetected = Physics2D.Raycast(transform.position, Vector2.up, ceilingCheckDistance, ceilingLayer);
        isNearWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

        // if (isGrounded)
        // {
        //     canDoubleJump = true; // Reset double jump when grounded
        // }

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
        anim.SetBool("isDashing", isDashing);

        if (rb.linearVelocity.y < rollVelocityThreshold)
        {
            anim.SetBool("canRoll", true);
        }
    }


    private void RollAnimFinished()
    {
        anim.SetBool("canRoll", false);
    }


    private void SetHorizontalVelocity(float value)
    {
        // prevent setting velocity if the Rigidbody is static (e.g., during ledge climbing)
        // No warning of can't set velocity on static body
        if (rb.bodyType == RigidbodyType2D.Static) return;

        rb.linearVelocity = new Vector2(
            value,
            rb.linearVelocity.y
        );
    }


    private void Jump()
    {
        isGrounded = false;
        canSliding = false;
        isSliding = false;
        anim.SetBool("canRoll", false);

        rb.linearVelocityY = jumpForce;
    }


    private void DoubleJump()
    {
        canDoubleJump = false;
        anim.SetBool("canRoll", false);

        rb.linearVelocityY = doubleJumpForce;
    }


    private void SlidingCheck()
    {
        if (isSliding)
        {
            slideTimerCounter = Mathf.Max(0, slideTimerCounter - Time.deltaTime);

            if (ceilingDetected)
                Debug.Log("ceiling Detected");

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
            rb.linearVelocity = Vector2.zero;
            if (!canDash){
                canDash = true;
                hitGroundAfterDash = true;
                sr.color = GameManager.instance.playerColor;
            }

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
        StartCoroutine(EnableLedgeGrabRoutine());
    }

    private IEnumerator EnableLedgeGrabRoutine()
    {
        yield return new WaitForSeconds(1f);
        canGrabLedge = true;
    }

    #endregion


    #region Speed Controller
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
    #endregion


    #region Damage and Knockback
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
        canKnocked = false;

        for (int i = 0; i < 12; i++)
        {
            Color originalColor;
            Color darkerColor;

            if (hitGroundAfterDash)
            {
                originalColor = GameManager.instance.playerColor;
                darkerColor = new Color(originalColor.r, originalColor.g, originalColor.b, .5f);
            }
            else
            {
                originalColor = GameManager.instance.playerDashColor;
                darkerColor = new Color(originalColor.r, originalColor.g, originalColor.b, .5f);
            }

            sr.color = darkerColor;
            yield return new WaitForSeconds(.2f);
            sr.color = originalColor;
            yield return new WaitForSeconds(.2f);
        }

        // Reset to proper color based on current state
        if (hitGroundAfterDash)
            sr.color = GameManager.instance.playerColor;
        else
            sr.color = GameManager.instance.playerDashColor;

        canKnocked = true;
    }


    private IEnumerator Die()
    {
        movementDisabled = true;
        canKnocked = false;
        isDead = true;

        rb.linearVelocity = deathBackDir;
        anim.SetBool("isDead", true);

        Time.timeScale = .6f;

        yield return new WaitForSeconds(.5f);
        Time.timeScale = 1f;

        yield return new WaitForSeconds(.5f);
        rb.linearVelocityX = 0f;

        yield return new WaitForSeconds(1f);
        rb.linearVelocityY = 0f;

        GameManager.instance.GameEnded();
    }


    public void Damage()
    {
        if (!canKnocked)
        {
            return;
        }

        if (extraLife)
        {
            extraLifeCount = Math.Max(0, extraLifeCount - 1);

            if (extraLifeCount == 0){
                extraLife = false;
                
                if (!isRecharging)
                {
                    StartCoroutine(RechargeExtraLife());
                }
            }

            Knockback();

        }
        else
        {
            StartCoroutine(Die());
        }
    }


    private IEnumerator RechargeExtraLife()
    {
        isRecharging = true;
        yield return new WaitForSeconds(GameManager.instance.extraLifeRechargeTime);
        extraLife = true;
        extraLifeCount += 1;
        isRecharging = false;
    }
    #endregion


    # region Dash/Double Dash
    private void DashingCheck()
    {
        dashTimerCounter = Mathf.Max(0, dashTimerCounter - Time.deltaTime);
        if (isDashing)
        {
            rb.linearVelocityY = 0; // Make dash straight forward

            if (dashTimerCounter <= 0 || isNearWall)
            {
                isDashing = false;
                canDash = false;
                dashCooldownTimerCounter = dashCooldownTimer;
                dashTimerCounter = dashTimer;
                anim.SetBool("canRoll", false);
            }
        }

        if (!canDash)
        {
            dashCooldownTimerCounter = Mathf.Max(0, dashCooldownTimerCounter - Time.deltaTime);

            if (isGrounded && !hitGroundAfterDash)
                hitGroundAfterDash = true;

            if (dashCooldownTimerCounter <= 0 && hitGroundAfterDash)
            {
                canDash = true;
                sr.color = GameManager.instance.playerColor;
            }
        }
    }

    public void rechargeDashViaDiamond()
    {
        canDash = true;
        hitGroundAfterDash = true; // technically not hitting ground it just hit a diamond
        isDashing = false;
        rb.linearVelocity = diamondHitForce;
        sr.color = GameManager.instance.playerColor;
    }

    #endregion
}

