using UnityEngine;

public class DynamicPlatformPivot : MonoBehaviour
{
    public enum PhaseASide
    {
        Right,
        Left
    }

    [Header("Platform References")]
    public Transform phaseA; // Phase A visual
    public Transform phaseB; // Phase B visual

    [Header("Settings")]
    public float totalLength = 5f;   // Total horizontal length of the full platform
    public float speed = 2f;         // Speed of the hacksaw motion
    public PhaseASide phaseASide = PhaseASide.Right; // Choose which side Phase A is on

    private float progress = 0f;     // 0 = Phase A full, 1 = Phase B full
    private int direction = 1;       // 1 = A contracts, -1 = B contracts

    private Vector3 phaseAScaleOriginal;
    private Vector3 phaseBScaleOriginal;

    void Start()
    {
        if (phaseA == null || phaseB == null)
        {
            Debug.LogError("Assign both Phase A and Phase B!");
            enabled = false;
            return;
        }

        // Store original scales to keep vertical/thickness consistent
        phaseAScaleOriginal = phaseA.localScale;
        phaseBScaleOriginal = phaseB.localScale;

        UpdatePlatform();
    }

    void Update()
    {
        // Move progress between 0 and 1
        progress += direction * speed * Time.deltaTime / totalLength;

        // Clamp and reverse direction at bounds
        if (progress >= 1f)
        {
            progress = 1f;
            direction *= -1;
        }
        else if (progress <= 0f)
        {
            progress = 0f;
            direction *= -1;
        }

        UpdatePlatform();
    }

    void UpdatePlatform()
    {
        // Calculate horizontal lengths for both sides
        float phaseALength = totalLength * (1f - progress);
        float phaseBLength = totalLength * progress;

        // Update scales only on X-axis
        phaseA.localScale = new Vector3(phaseALength, phaseAScaleOriginal.y, phaseAScaleOriginal.z);
        phaseB.localScale = new Vector3(phaseBLength, phaseBScaleOriginal.y, phaseBScaleOriginal.z);

        // Adjust positions based on which side Phase A is on
        if (phaseASide == PhaseASide.Right)
        {
            // Phase A extends to the right
            phaseA.localPosition = new Vector3(phaseALength / 2f, 0f, 0f);
            // Phase B extends to the left
            phaseB.localPosition = new Vector3(-phaseBLength / 2f, 0f, 0f);
        }
        else
        {
            // Phase A extends to the left
            phaseA.localPosition = new Vector3(-phaseALength / 2f, 0f, 0f);
            // Phase B extends to the right
            phaseB.localPosition = new Vector3(phaseBLength / 2f, 0f, 0f);
        }
    }
}






