using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInitializer : MonoBehaviour
{
    [Header("Fade Transition Settings")]
    public bool createFadeTransition = true;
    
    private static bool isInitialized = false;

    private void Awake()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            Debug.Log("First time initialization of game managers...");
            InitializeManagers();
        }
        else
        {
            Debug.Log("Game already initialized, skipping");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Double-check that managers exist in Start
        if (GameManager.Instance == null || SceneController.Instance == null)
        {
            Debug.LogWarning("Managers missing in Start, reinitializing...");
            InitializeManagers();
        }
    }

    private void InitializeManagers()
    {
        // CreatingGameManager
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObj);
            Debug.Log("GameManager created");
        }
        else
        {
            Debug.Log("GameManager already exists");
        }

        // Create SceneController
        if (SceneController.Instance == null)
        {
            GameObject sceneControllerObj = new GameObject("SceneController");
            sceneControllerObj.AddComponent<SceneController>();
            DontDestroyOnLoad(sceneControllerObj);
            Debug.Log("SceneController created");
        }
        else
        {
            Debug.Log("SceneController already exists");
        }
        
        // Create FadeTransition canvas if enabled and doesnt exist
        if (createFadeTransition && FadeTransition.Instance == null)
        {
            CreateFadeTransitionCanvas();
        }

        Debug.Log("Game managers initialized successfully");
    }
    
    private void CreateFadeTransitionCanvas()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // make sure its rendered on top of everything
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add Graphic Raycaste
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create blackimage for fading
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        // Set up the image to cover the entire screen
        RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Add and configure the image component
        Image image = imageObj.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0); 
        image.raycastTarget = false; 
        
        // Make the image solid black
        image.color = Color.black;
        // But fully transparent
        Color color = image.color;
        color.a = 0;
        image.color = color;
        
        // Add the FadeTransition component to the canvas
        FadeTransition fadeTransition = canvasObj.AddComponent<FadeTransition>();
        fadeTransition.fadeImage = image;
        
        Debug.Log("FadeTransition canvas created");
    }
} 