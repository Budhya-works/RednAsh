using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class playermovment : MonoBehaviour
{
    // === Event for rotating platforms to listen to phase changes ===
    public static System.Action<bool> OnPhaseChanged;

    [Header("Player Settings")]
    public float speed = 5f;
    public float jumpForce = 10f;

    [Header("Second Player Settings")]
    public GameObject otherPlayer;

    [Header("Phase Settings")]
    public float fadeDuration = 0.7f;
    public Color phaseAColor;
    public Color phaseBColor;

    [Header("Level Inversion Settings")]
    public float levelMidpointX = 0f; // Gravity inversion threshold
    public float levelMidpointY = 0f; // Horizontal inversion threshold

    [Header("Overlap Settings")]
    public Vector2 overlapBoxSize = new Vector2(0.8f, 1.5f);
    public float ghostDuration = 0.3f;

    [Header("Scene Reset Settings")]
    public bool resetOnOutOfView = true;
    public float extraBoundary = 1f;

    [Header("Rotation Settings")]
    public float rotationDuration = 0.25f; // Speed of flip animation
    private bool isRotating = false;

    [Header("Landing Jerk Settings")]
    public float landingJerkForce = 6f;       // Strength of the jerk impulse (tweak in inspector)
    public float landingJerkCooldown = 0.12f; // Prevent re-triggering immediately

    private Rigidbody2D rb;
    private Rigidbody2D otherRb;
    private Collider2D playerCollider;
    private Collider2D otherPlayerCollider;
    private Camera mainCamera;

    private bool playerGrounded = false;
    private bool otherGrounded = false;
    private bool canJump = false;

    private bool isPhaseA = true;
    private bool isGravityInverted = false;
    private bool isHorizontalInverted = false;

    // Track current platform
    private LiftingPlatform currentPlatformUnderPlayer = null;

    // Track landing jerk state
    private bool isLandingJerkOnCooldown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        if (otherPlayer != null)
        {
            otherRb = otherPlayer.GetComponent<Rigidbody2D>();
            otherPlayerCollider = otherPlayer.GetComponent<Collider2D>();
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Tag your camera as 'MainCamera'.");
            enabled = false;
            return;
        }

        SetPhaseState(isPhaseA, true);
        SetBackgroundColor(isPhaseA, true);
    }

    void Update()
    {
        HandleHorizontalMovement();
        CheckForGravityAndDirectionInversion();

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            PerformJumpAndPhaseSwitch();
        }

        CheckPlayerOutOfCamera();
    }

    // === MOVEMENT ===
    void HandleHorizontalMovement()
    {
        float move = Input.GetAxis("Horizontal");
        if (isHorizontalInverted) move *= -1f;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        if (otherRb != null)
            otherRb.linearVelocity = new Vector2(move * speed, otherRb.linearVelocity.y);
    }

    // === JUMP AND PHASE SWITCH ===
    void PerformJumpAndPhaseSwitch()
    {
        // Reset vertical velocity before applying jump to make jump consistent
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        if (otherRb != null) otherRb.linearVelocity = new Vector2(otherRb.linearVelocity.x, 0f);

        // Determine jump direction based on gravity
        Vector2 jumpDirection = (rb.gravityScale >= 0f) ? Vector2.up : Vector2.down;

        // Apply impulse for jump (consistent jerk when jumping)
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        if (otherRb != null) otherRb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        // Switch phase
        isPhaseA = !isPhaseA;
        OnPhaseChanged?.Invoke(isPhaseA);
        SetPhaseState(isPhaseA, false);
        SetBackgroundColor(isPhaseA, false);

        TemporarilyIgnoreOverlappingObjects();

        canJump = false;
    }

    // === COLLISION HANDLING ===
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundCollision(collision))
        {
            // compute average contact normal to confirm floor-like contact
            Vector2 avgNormal = Vector2.zero;
            int count = collision.contactCount;
            if (count > 0)
            {
                foreach (ContactPoint2D cp in collision.contacts)
                    avgNormal += cp.normal;
                avgNormal /= (float)count;
            }

            // Determine whether this contact is a "landing" contact relative to gravity
            bool contactIsFloor = false;
            if (rb.gravityScale >= 0f)
                contactIsFloor = avgNormal.y > 0.5f; // normal pointing up -> landed on top
            else
                contactIsFloor = avgNormal.y < -0.5f; // normal pointing down -> landed on bottom (inverted)

            // If player's collider was involved and contact is a floor, mark grounded and trigger jerk
            if (collision.otherCollider == playerCollider)
            {
                if (contactIsFloor)
                {
                    playerGrounded = true;
                    TriggerLandingJerk();
                }
                else
                {
                    // collision but not a floor contact (e.g., side) -> still treat as not grounded for jerk
                    playerGrounded = true; // keep earlier behavior for canJump, but don't jerk
                }
            }
            else if (otherPlayerCollider != null && collision.otherCollider == otherPlayerCollider)
            {
                if (contactIsFloor)
                {
                    otherGrounded = true;
                    // optionally apply jerk to other player: currently not applying
                }
                else
                {
                    otherGrounded = true;
                }
            }

            UpdateCanJump();
        }

        // Check if it's a lifting platform
        LiftingPlatform platform = collision.gameObject.GetComponent<LiftingPlatform>();
        if (platform != null)
        {
            // compute contact normal for platform as well
            Vector2 avgNormal = Vector2.zero;
            int count = collision.contactCount;
            if (count > 0)
            {
                foreach (ContactPoint2D cp in collision.contacts)
                    avgNormal += cp.normal;
                avgNormal /= (float)count;
            }

            bool contactIsFloor = (rb.gravityScale >= 0f) ? avgNormal.y > 0.5f : avgNormal.y < -0.5f;

            currentPlatformUnderPlayer = platform;
            UpdateCanJump();

            if (contactIsFloor)
                TriggerLandingJerk();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundCollision(collision))
        {
            if (collision.otherCollider == playerCollider)
                playerGrounded = false;
            else if (otherPlayerCollider != null && collision.otherCollider == otherPlayerCollider)
                otherGrounded = false;

            UpdateCanJump();
        }

        // Leaving platform
        LiftingPlatform platform = collision.gameObject.GetComponent<LiftingPlatform>();
        if (platform != null && platform == currentPlatformUnderPlayer)
        {
            currentPlatformUnderPlayer = null;
            UpdateCanJump();
        }
    }

    // === SPIKE DETECTION ===
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // === CAN JUMP CHECK ===
    void UpdateCanJump()
    {
        canJump = playerGrounded || otherGrounded || currentPlatformUnderPlayer != null;
    }

    bool IsGroundCollision(Collision2D collision)
    {
        return collision.gameObject.CompareTag("Phase A") ||
               collision.gameObject.CompareTag("Phase B") ||
               collision.gameObject.CompareTag("Neutral");
    }

    // === LANDING JERK ===
    void TriggerLandingJerk()
    {
        // Only trigger if not cooling down
        if (isLandingJerkOnCooldown) return;

        StartCoroutine(ApplyLandingJerk());
    }

    IEnumerator ApplyLandingJerk()
    {
        isLandingJerkOnCooldown = true;

        // Direction toward the ground (works for normal and inverted gravities)
        Vector2 jerkDirection = (rb.gravityScale >= 0f) ? Vector2.down : Vector2.up;

        // Apply a short impulse for a clear, noticeable "thud"
        rb.AddForce(jerkDirection * landingJerkForce, ForceMode2D.Impulse);
        if (otherRb != null) otherRb.AddForce(jerkDirection * landingJerkForce, ForceMode2D.Impulse);

        // wait the cooldown, then allow another jerk
        yield return new WaitForSeconds(landingJerkCooldown);
        isLandingJerkOnCooldown = false;
    }

    // === PHASE LOGIC ===
    void SetPhaseState(bool phaseAActive, bool instant)
    {
        GameObject[] phaseAObjects = GameObject.FindGameObjectsWithTag("Phase A");
        GameObject[] phaseBObjects = GameObject.FindGameObjectsWithTag("Phase B");

        foreach (GameObject obj in phaseAObjects)
            ToggleObject(obj, phaseAActive, instant);

        foreach (GameObject obj in phaseBObjects)
            ToggleObject(obj, !phaseAActive, instant);
    }

    void ToggleObject(GameObject obj, bool isActive, bool instant)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Collider2D col = obj.GetComponent<Collider2D>();

        if (sr != null)
        {
            if (instant)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, isActive ? 1f : 0f);
                sr.enabled = isActive;
            }
            else
            {
                StartCoroutine(FadeObject(sr, isActive, fadeDuration));
            }
        }

        if (col != null) col.enabled = isActive;
    }

    IEnumerator FadeObject(SpriteRenderer sr, bool fadeIn, float duration)
    {
        sr.enabled = true;
        float startAlpha = sr.color.a;
        float targetAlpha = fadeIn ? 1f : 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, targetAlpha);
        if (!fadeIn) sr.enabled = false;
    }

    void SetBackgroundColor(bool phaseAActive, bool instant)
    {
        Color targetColor = phaseAActive ? phaseBColor : phaseAColor;

        if (instant)
            mainCamera.backgroundColor = targetColor;
        else
            StartCoroutine(FadeBackgroundColor(mainCamera, targetColor, fadeDuration));
    }

    IEnumerator FadeBackgroundColor(Camera cam, Color targetColor, float duration)
    {
        Color startColor = cam.backgroundColor;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            cam.backgroundColor = Color.Lerp(startColor, targetColor, t / duration);
            yield return null;
        }
        cam.backgroundColor = targetColor;
    }

    // === GRAVITY & HORIZONTAL INVERSION ===
    void CheckForGravityAndDirectionInversion()
    {
        bool newGravityState = transform.position.x > levelMidpointX;
        if (newGravityState != isGravityInverted)
        {
            isGravityInverted = newGravityState;
            rb.gravityScale = isGravityInverted ? -1f : 1f;
            if (otherRb != null) otherRb.gravityScale = rb.gravityScale;

            // Trigger rotation animation when gravity flips
            StartCoroutine(RotatePlayerOnGravityChange());
        }

        isHorizontalInverted = transform.position.y > levelMidpointY;
    }

    // === Smooth Rotation Animation ===
    IEnumerator RotatePlayerOnGravityChange()
    {
        if (isRotating) yield break; // Prevent overlapping animations
        isRotating = true;

        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, (rb.gravityScale >= 0f) ? 0f : 180f);

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation; // Snap to final rotation
        isRotating = false;
    }

    // === CAMERA OUT OF BOUNDS RESET ===
    void CheckPlayerOutOfCamera()
    {
        if (!resetOnOutOfView) return;

        Vector3 camPosition = mainCamera.transform.position;
        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        float left = camPosition.x - (camWidth / 2f) - extraBoundary;
        float right = camPosition.x + (camWidth / 2f) + extraBoundary;
        float bottom = camPosition.y - (camHeight / 2f) - extraBoundary;
        float top = camPosition.y + (camHeight / 2f) + extraBoundary;

        if (IsOutOfBounds(transform.position, left, right, bottom, top) ||
            (otherPlayer != null && IsOutOfBounds(otherPlayer.transform.position, left, right, bottom, top)))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    bool IsOutOfBounds(Vector3 pos, float left, float right, float bottom, float top)
    {
        return pos.x < left || pos.x > right || pos.y < bottom || pos.y > top;
    }

    // === TEMP COLLISION IGNORE ===
    void TemporarilyIgnoreOverlappingObjects()
    {
        Collider2D[] overlapsMain = Physics2D.OverlapBoxAll(transform.position, overlapBoxSize, 0f);
        HandleOverlaps(overlapsMain, playerCollider);

        if (otherPlayer != null)
        {
            Collider2D[] overlapsOther = Physics2D.OverlapBoxAll(otherPlayer.transform.position, overlapBoxSize, 0f);
            HandleOverlaps(overlapsOther, otherPlayerCollider);
        }
    }

    void HandleOverlaps(Collider2D[] overlaps, Collider2D sourceCol)
    {
        foreach (Collider2D col in overlaps)
        {
            if (col == null) continue;
            if (col.CompareTag("Phase A") || col.CompareTag("Phase B"))
                StartCoroutine(TemporarilyIgnoreCollider(sourceCol, col));
        }
    }

    IEnumerator TemporarilyIgnoreCollider(Collider2D playerCol, Collider2D phaseCol)
    {
        Physics2D.IgnoreCollision(playerCol, phaseCol, true);
        yield return new WaitForSeconds(ghostDuration);
        Physics2D.IgnoreCollision(playerCol, phaseCol, false);
    }
}





