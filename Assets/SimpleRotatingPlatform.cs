using UnityEngine;

public class SimpleRotatingPlatform : MonoBehaviour
{
    [Header("Child Objects")]
    public GameObject phaseAObj;
    public GameObject phaseBObj;

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // degrees per second
    private int rotationDirection = 1; // 1 = CCW, -1 = CW

    private void OnEnable()
    {
        // Subscribe to phase change event from player
        playermovment.OnPhaseChanged += HandlePhaseChange;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        playermovment.OnPhaseChanged -= HandlePhaseChange;
    }

    private void Update()
    {
        // Rotate both children around their own center
        float rotateAmount = rotationSpeed * rotationDirection * Time.deltaTime;
        phaseAObj.transform.Rotate(Vector3.forward, rotateAmount);
        phaseBObj.transform.Rotate(Vector3.forward, rotateAmount);
    }

    /// <summary>
    /// Called when the player switches phase.
    /// Swaps visibility and collider states, and reverses rotation direction.
    /// </summary>
    private void HandlePhaseChange(bool isPhaseA)
    {
        // Reverse rotation direction
        rotationDirection *= -1;

        // Phase A becomes active
        if (isPhaseA)
        {
            SetPhaseState(phaseAObj, true);
            SetPhaseState(phaseBObj, false);
        }
        // Phase B becomes active
        else
        {
            SetPhaseState(phaseAObj, false);
            SetPhaseState(phaseBObj, true);
        }
    }

    /// <summary>
    /// Enables or disables visibility and collider of a phase object.
    /// </summary>
    private void SetPhaseState(GameObject obj, bool active)
    {
        if (obj == null) return;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Collider2D col = obj.GetComponent<Collider2D>();

        if (sr != null) sr.enabled = active;
        if (col != null) col.enabled = active;
    }
}











