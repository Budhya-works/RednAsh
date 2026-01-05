using UnityEngine;

public class LiftingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform pointA;               // Start position
    public Transform pointB;               // End position
    public float moveSpeed = 2f;           // Platform speed
    public bool moveOnStart = false;       // Start moving automatically?

    private Vector3 targetPosition;        // Current target
    private bool isMoving = false;
    private bool hasTriggered = false;
    private Transform playerOnPlatform = null;

    private Vector3 lastPlatformPosition;

    void Start()
    {
        transform.position = pointA.position;
        targetPosition = pointB.position;
        lastPlatformPosition = transform.position;

        if (moveOnStart)
        {
            StartMovement();
        }
    }

    void Update()
    {
        if (isMoving)
        {
            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Stop at point when reached
            if (Vector3.Distance(transform.position, targetPosition) <= 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }

        // Move the player with the platform smoothly
        if (playerOnPlatform != null)
        {
            Vector3 deltaMovement = transform.position - lastPlatformPosition;
            playerOnPlatform.position += deltaMovement;
        }

        lastPlatformPosition = transform.position;
    }

    public void StartMovement()
    {
        isMoving = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = collision.transform;

            // Trigger movement only once if not moving on start
            if (!hasTriggered && !moveOnStart)
            {
                StartMovement();
                hasTriggered = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = null;
        }
    }
}




