using UnityEngine;
using System.Collections;

public class SlantingPlatform : MonoBehaviour
{
    [Header("Phase Objects")]
    public GameObject phaseA;
    public GameObject phaseB;

    [Header("Phase Angles (Degrees)")]
    public float phaseAAngle = 15f;
    public float phaseBAngle = -15f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 200f; // Degrees per second

    private bool isPhaseA = true;
    private float targetAngle;

    void Start()
    {
        if (phaseA == null || phaseB == null)
        {
            Debug.LogError("Assign Phase A and Phase B objects!");
            enabled = false;
            return;
        }

        // Initialize
        isPhaseA = true;
        targetAngle = phaseAAngle;
        UpdateVisuals(true);

        // Subscribe to player phase switch
        playermovment.OnPhaseChanged += OnPlayerPhaseChanged;
    }

    void OnDestroy()
    {
        playermovment.OnPhaseChanged -= OnPlayerPhaseChanged;
    }

    void Update()
    {
        // Smoothly rotate to target angle
        float currentZ = transform.eulerAngles.z;
        float newZ = Mathf.MoveTowardsAngle(currentZ, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    void OnPlayerPhaseChanged(bool isPhaseANow)
    {
        isPhaseA = isPhaseANow;
        targetAngle = isPhaseA ? phaseAAngle : phaseBAngle;
        UpdateVisuals(false);
    }

    void UpdateVisuals(bool instant)
    {
        // Phase A active?
        phaseA.SetActive(isPhaseA);
        phaseB.SetActive(!isPhaseA);
    }
}

