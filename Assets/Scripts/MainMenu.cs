using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Game Settings")]
    public string gameStartScene = "FarmScene";
    public string startSpawnPoint = "MainSpawn";  
    
    [Header("Menu Buttons")]
    public Button startGameButton;
    public Button optionsButton;
    public Button quitButton;
    
    private static bool isFirstGameRun = true;
    
    void Start()
    {
        // to help debug
        Debug.Log("MainMenu Start() called");
        
        // Ensure the game initializer exists
        if (FindObjectOfType<GameInitializer>() == null)
        {
            Debug.Log("GameInitializer not found, creating one...");
            GameObject initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
            
            // Only check the fade system if this is not the first run
            if (!isFirstGameRun)
            {
                Invoke("CheckFadeSystem", 0.5f);
            }
        }
        else
        {
            Debug.Log("GameInitializer already exists");
            
            // Only check the fade system if this is not the first run
            if (!isFirstGameRun)
            {
                CheckFadeSystem();
            }
            else
            {
                // If this is the first run make sure any fade image is fully transparent
                if (FadeTransition.Instance != null)
                {
                    // Force transparency on first run
                    FadeTransition.Instance.ForceTransparent();
                    Debug.Log("First game run: Forced fade transition to transparent");
                }
            }
        }
        
        // After the first run of the game, we'll set this to false
        if (isFirstGameRun)
        {
            isFirstGameRun = false;
            Debug.Log("First game run flag set to false for future scene loads");
        }
        
        // Setup button listeners programmatically
        SetupButtonListeners();
    }
    
    private void SetupButtonListeners()
    {
        // Setup Start Game button
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners(); 
            startGameButton.onClick.AddListener(StartGame);
            Debug.Log("Start Game button listener added");
        }
        else
        {
            Debug.LogWarning("Start Game button not assigned in inspector!");
        }
        
        // Setup Quit button
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners(); 
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("Quit button listener added");
        }
        else
        {
            Debug.LogWarning("Quit button not assigned in inspector!");
        }
        
        // Debug button configs
        Debug.Log("Button setup complete");
    }
    
    // Make sure the fade system is working on the main menu
    private void CheckFadeSystem()
    {
        if (FadeTransition.Instance != null)
        {
            Debug.Log("FadeTransition found, fading in...");
            // Fade in when main menu starts
            StartCoroutine(FadeTransition.Instance.FadeToClear());
        }
        else
        {
            Debug.LogWarning("FadeTransition not found in main menu!");
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame() called - Attempting to load scene: " + gameStartScene);
        
        // Check if scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == gameStartScene)
            {
                sceneExists = true;
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"Scene '{gameStartScene}' does not exist in build settings! Please add it.");
            return;
        }
        
        // Use SceneController if it exists
        if (SceneController.Instance != null)
        {
            Debug.Log("Using SceneController to change scene");
            SceneController.Instance.ChangeScene(gameStartScene, startSpawnPoint);
        }
        else
        {
            // Fallback to direct scene loading
            Debug.LogWarning("SceneController not found! Using direct scene loading.");
            
            // Save spawn point for the next scene
            PlayerPrefs.SetString("SpawnPoint", startSpawnPoint);
            PlayerPrefs.Save();
            
            // First fade to black if fade transition exists
            if (FadeTransition.Instance != null)
            {
                StartCoroutine(FadeToBlackThenLoadScene());
            }
            else
            {
                SceneManager.LoadScene(gameStartScene);
            }
        }
    }
    
    private IEnumerator FadeToBlackThenLoadScene()
    {
        // Fade to black first
        yield return FadeTransition.Instance.FadeToBlack();
        
        // Then load scene
        SceneManager.LoadScene(gameStartScene);
    }

    public void OpenOptions()
    {
        Debug.Log("Options button clicked - functionality not implemented yet");
        // Implement options menu functionality here
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame() called from MainMenu");
        
        // Use the shared GameUtilities class for quitting
        GameUtilities.QuitGame(true);
    }
}
