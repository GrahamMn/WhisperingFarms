using UnityEngine;
using System.Collections;


// Shared utility functions for game operations like quitting, scene transitions, etc.

public static class GameUtilities
{
    
    // Quit the game with an optional fade to black effect
    public static void QuitGame(bool fadeToBlack = true, float fadeDuration = 0.5f)
    {
        // Always reset time scale first to ensure animations and coroutines work properly
        Time.timeScale = 1f;
        
        if (fadeToBlack && FadeTransition.Instance != null)
        {
            // Start a coroutine using a MonoBehaviour instance
            MonoBehaviour runner = GetCoroutineRunner();
            if (runner != null)
            {
                runner.StartCoroutine(FadeAndQuit(fadeDuration));
            }
            else
            {
                QuitDirectly();
            }
        }
        else
        {
            QuitDirectly();
        }
    }

    
    // Immediately quit the game
    
    private static void QuitDirectly()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

 
    // Fade to black, then quit the game
  
    private static IEnumerator FadeAndQuit(float fadeDuration)
    {
        Debug.Log("Fading to black before quitting...");
        
        if (FadeTransition.Instance != null)
        {
            // Fade to black first
            yield return FadeTransition.Instance.FadeToBlack();
            
            // Small delay to see the black screen
            yield return new WaitForSeconds(0.2f);
        }
        
        // Then quit
        QuitDirectly();
    }

    
    // Return to the main menu with proper cleanup
    
    public static void ReturnToMainMenu(bool saveGameState = true)
    {
        // Always reset time scale first to ensure proper scene transitions
        Time.timeScale = 1f;
        
        if (saveGameState && GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameState();
        }
        
        // Use the SceneController if available
        if (SceneController.Instance != null)
        {
            SceneController.Instance.ChangeScene("MainMenu", "MainSpawn");
        }
        else
        {
            // Fallback to direct scene loading
            Debug.LogWarning("SceneController not found! Using direct scene loading.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    
    // Find an active MonoBehaviour to use for running coroutines
   
    private static MonoBehaviour GetCoroutineRunner()
    {
        // Try to find common persistent managers first
        if (FadeTransition.Instance != null) 
            return FadeTransition.Instance;
            
        if (SceneController.Instance != null) 
            return SceneController.Instance;
            
        if (GameManager.Instance != null) 
            return GameManager.Instance;
            
        // If no persistent managers are found, try to find any active MonoBehaviour
        MonoBehaviour[] activeObjects = Object.FindObjectsOfType<MonoBehaviour>();
        if (activeObjects.Length > 0)
            return activeObjects[0];
            
        Debug.LogError("Could not find any active MonoBehaviour to run coroutines!");
        return null;
    }
} 