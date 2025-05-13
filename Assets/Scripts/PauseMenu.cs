using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Start()
    {
        // Hide the pause menu when the game starts
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading main menu from pause menu");
        // Use the GameUtilities for returning to the main menu
        GameUtilities.ReturnToMainMenu(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game from pause menu");
        
        // First hide the pause UI
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Then use the GameUtilities for quitting
        // The utility will handle restoring time scale
        GameUtilities.QuitGame(true);
    }
}
