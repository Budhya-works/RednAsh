using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadePanel;                  // Fullscreen UI Image for fading
    public float fadeDuration = 0.7f;

    [Header("Loading Text Settings")]
    [Tooltip("Assign a TMP Text that already has the message you want to display")]
    public TextMeshProUGUI loadingText;

    public float loadingTextDelay = 0.5f;      // Delay before text appears
    public float loadingTextVisibleTime = 1.5f; // How long text stays visible

    private bool isTransitioning = false;

    void Start()
    {
        // Fade in automatically at scene start
        StartCoroutine(FadeInAtStart());
    }

    /// <summary>
    /// Starts a transition to another scene.
    /// </summary>
    public void StartSceneTransition(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(SceneTransitionSequence(sceneName));
        }
    }

    // --- Fade in when a new scene starts ---
    private IEnumerator FadeInAtStart()
    {
        fadePanel.gameObject.SetActive(true);

        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;

        float t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        fadePanel.gameObject.SetActive(false);
        Time.timeScale = 1f; // Safety reset
    }

    // --- Main transition sequence ---
    private IEnumerator SceneTransitionSequence(string sceneName)
    {
        isTransitioning = true;

        // Step 1: Freeze the game
        Time.timeScale = 0f;

        // Step 2: Fade to black
        fadePanel.gameObject.SetActive(true);
        Color color = fadePanel.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        // Step 3: Small delay before showing the loading text
        yield return new WaitForSecondsRealtime(loadingTextDelay);

        // Step 4: Show TMP text (using whatever text is already set in the editor)
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Loading TextMeshProUGUI is not assigned in the SceneFader!");
        }

        // Step 5: Keep it visible for a bit
        yield return new WaitForSecondsRealtime(loadingTextVisibleTime);

        // Step 6: Hide the text
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        // Step 7: Load the new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Step 8: Resume game
        Time.timeScale = 1f;

        // Step 9: Fade in the new scene
        yield return StartCoroutine(FadeInAtStart());

        isTransitioning = false;
    }
}






