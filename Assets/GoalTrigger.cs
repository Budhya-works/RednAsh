using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextSceneName = "Level2";

    private SceneFader sceneFader;
    private bool hasTriggered = false; // Prevents double triggering

    void Start()
    {
        // Updated to avoid obsolete warning
        sceneFader = FindFirstObjectByType<SceneFader>();

        if (sceneFader == null)
        {
            Debug.LogError("SceneFader not found in the scene! Make sure there is one in the Canvas.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger only once, and only when player enters
        if (!hasTriggered && other.CompareTag("Player") && sceneFader != null)
        {
            hasTriggered = true; // Prevents multiple calls
            sceneFader.StartSceneTransition(nextSceneName);
        }
    }
}








