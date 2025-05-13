using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    private bool isSceneTransitionInProgress = false;
    private float sceneTransitionCooldown = 0.5f; 
    private float lastSceneTransitionTime = 0f;
    private static bool isFirstGameLoad = true;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("SceneController initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Check if a scene transition is currently allowed
    public bool CanTransitionScene()
    {
        // Don't allow transitions if one is already in progress
        if (isSceneTransitionInProgress)
        {
            Debug.Log("Scene transition already in progress, ignoring request");
            return false;
        }
        
        // Check if cooldown timer has elapsed
        float timeSinceLastTransition = Time.time - lastSceneTransitionTime;
        if (timeSinceLastTransition < sceneTransitionCooldown)
        {
            Debug.Log($"Scene transition on cooldown ({timeSinceLastTransition:F1}s/{sceneTransitionCooldown:F1}s), ignoring request");
            return false;
        }
        
        return true;
    }
    
    public void ChangeScene(string sceneName, string spawnPointName)
    {
        // Check if we can transition
        if (!CanTransitionScene())
            return;
            
        Debug.Log($"Changing to scene: {sceneName}, spawn point: {spawnPointName}");
        
        // Mark transition as in progress
        isSceneTransitionInProgress = true;
        lastSceneTransitionTime = Time.time;
        
        // Save current game state before changing scenes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameState();
            Debug.Log("Game state saved before scene change");
        }
        else
        {
            Debug.LogError("GameManager instance not found, cannot save state!");
        }
        
        // Set spawn point for the next scene
        PlayerPrefs.SetString("SpawnPoint", spawnPointName);
        PlayerPrefs.Save();
        
        // Start the scene transition with fade effect
        StartCoroutine(TransitionToScene(sceneName));
    }
    
    private IEnumerator TransitionToScene(string sceneName)
    {
        // Check if FadeTransition exists
        if (FadeTransition.Instance != null)
        {
            // Fade to black before changing scene
            yield return FadeTransition.Instance.FadeToBlack();
        }
        
        // Load the scene
        SceneManager.LoadScene(sceneName);
        
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        // Check if this is the first game
        if (isFirstGameLoad)
        {
            Debug.Log("First game load detected - skipping fade transitions");
            isFirstGameLoad = false;
            isSceneTransitionInProgress = false;
            
            // Force any fade image to be transparent
            if (FadeTransition.Instance != null)
            {
                FadeTransition.Instance.ForceTransparent();
            }
            
            // Load game state immediately without fade
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGameState();
            }
            return;
        }
        
        // Load game state after the scene is loaded and all objects are initialized
        if (GameManager.Instance != null)
        {
            
            StartCoroutine(LoadGameStateDelayed(0.2f));
        }
        else
        {
            Debug.LogError("GameManager instance not found during scene load!");
            
            // If no GameManager still fade back in
            if (FadeTransition.Instance != null)
            {
                StartCoroutine(FadeTransition.Instance.FadeToClear());
            }
            
            // Mark transition as completed
            isSceneTransitionInProgress = false;
        }
    }
    
    private System.Collections.IEnumerator LoadGameStateDelayed(float delay)
    {
        // Wait for delay
        yield return new WaitForSeconds(delay);
        
        // Wait for end of frame to ensure all objects are initialized
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Loading game state after delay");
        GameManager.Instance.LoadGameState();
        
        // Now fade back in from black
        if (FadeTransition.Instance != null)
        {
            yield return FadeTransition.Instance.FadeToClear();
        }
        
        // Mark transition as completed
        isSceneTransitionInProgress = false;
    }
} 