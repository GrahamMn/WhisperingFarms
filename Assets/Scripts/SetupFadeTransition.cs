using UnityEngine;
using UnityEngine.UI;

public class SetupFadeTransition : MonoBehaviour
{
    [Header("Step 1: Create a Canvas")]
    [TextArea(3, 10)]
    public string step1Instructions = "Create a new Canvas (UI > Canvas) and set it to Screen Space - Overlay with a sort order of 999 so it appears above everything. Name it 'FadeCanvas'.";
    
    [Header("Step 2: Add Image")]
    [TextArea(3, 10)]
    public string step2Instructions = "Add a new Image (UI > Image) as a child of the Canvas. Set its anchors to cover the entire screen (right click > Stretch). Name it 'FadeImage'. Make it BLACK.";
    
    [Header("Step 3: Add FadeTransition Component")]
    [TextArea(3, 10)]
    public string step3Instructions = "Add the FadeTransition script to the Canvas. Drag the FadeImage to the 'Fade Image' field in the inspector.";
    
    [Header("Step 4: Set Canvas to DontDestroyOnLoad")]
    [TextArea(3, 10)]
    public string step4Instructions = "The FadeTransition script will automatically set the canvas to DontDestroyOnLoad, so it persists between scenes.";
    
    [Header("Step 5: Set Initial State")]
    [TextArea(3, 10)]
    public string step5Instructions = "Set the initial alpha of the FadeImage to 0 (transparent) in the Color property. The script will handle this automatically on start, but it's good to set it manually as well.";
    
    [Header("Step 6: Add to GameInitializer")]
    [TextArea(4, 10)]
    public string step6Instructions = "Make sure your FadeCanvas is spawned at the start of the game. You can add it to your starting scene, or instantiate it from a prefab in your GameInitializer script. The FadeTransition component will handle making it a singleton.";
    
    [Header("Step 7: Test")]
    [TextArea(3, 10)]
    public string step7Instructions = "Play the game and test scene transitions. You should see a smooth fade to black, then fade back in after the scene loads.";
    

    void OnDrawGizmos()
    {
        // Just draw visible icon in the editor to make easy to find
        Gizmos.color = Color.black;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
    }
    
    void OnValidate()
    {
        // Validate component references if script is attached to a GameObject
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("FadeTransition should be attached to a Canvas!");
        }
        
        FadeTransition fadeTransition = GetComponent<FadeTransition>();
        if (fadeTransition == null)
        {
            Debug.LogWarning("FadeTransition component is missing!");
        }
        
        // Find the fade image
        Image[] images = GetComponentsInChildren<Image>();
        bool foundFadeImage = false;
        foreach (Image img in images)
        {
            if (img.gameObject.name == "FadeImage" || 
                (fadeTransition != null && fadeTransition.GetComponent<Image>() == img))
            {
                foundFadeImage = true;
                break;
            }
        }
        
        if (!foundFadeImage)
        {
            Debug.LogWarning("FadeImage not found in children!");
        }
    }
} 