using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button musicToggleButton;
    private AudioSource backgroundMusic;
    public void PlayGame1()
    {
        SceneManager.LoadSceneAsync("level1");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame2()
    {
        SceneManager.LoadSceneAsync("level2");
    }

    public void PlayGame3()
    {
        SceneManager.LoadSceneAsync("level3");
    }
    public void PlayGame4()
    {
        SceneManager.LoadSceneAsync("level4");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Start()
    {
        musicToggleButton.onClick.AddListener(ToggleMusic);

        GameObject musicObj = GameObject.FindWithTag("Music");
        if (musicObj != null)
        {
            backgroundMusic = musicObj.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("No GameObject with tag 'Music' found in the scene.");
        }
    }

    public void ToggleMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = !backgroundMusic.mute;
        }
    }
}
