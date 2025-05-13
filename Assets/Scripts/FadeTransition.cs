using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Instance { get; private set; }

    [SerializeField] public Image fadeImage;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float blackScreenDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    // Flag to control initial fade
    private static bool isFirstSceneLoad = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure the fade image is first transparent
            if (fadeImage != null)
            {
                // On first scene load, make sure starts fully transparent with no fade at all
                if (isFirstSceneLoad)
                {
                    Color color = fadeImage.color;
                    color.a = 0f;
                    fadeImage.color = color;
                    fadeImage.gameObject.SetActive(true);
                    isFirstSceneLoad = false;
                    Debug.Log("FadeTransition initialized with transparent image (first scene load)");
                }
                else
                {
                    Color color = fadeImage.color;
                    color.a = 0f;
                    fadeImage.color = color;
                    fadeImage.gameObject.SetActive(true);
                    Debug.Log("FadeTransition initialized with transparent image");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
   
    // Force the fade image to be completely transparent
    
    public void ForceTransparent()
    {
        if (fadeImage != null)
        {
            // Force alpha to 0
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            Debug.Log("Fade image forced to transparent");
        }
        else
        {
            Debug.LogWarning("Cannot force transparency - fade image is null");
        }
    }

    // Fade to black, wait for scene change and loading, then fade back in
    public IEnumerator DoFadeTransition(System.Action onFadeComplete = null)
    {
        // Make sure we have a fade image
        if (fadeImage != null)
        {
            // Fade to black
            yield return FadeToBlack();
            
            // Wait for the specified duration with black screen
            yield return new WaitForSeconds(blackScreenDuration);
            
            // Fade back to clear
            yield return FadeToClear();
            
            onFadeComplete?.Invoke();
        }
        else
        {
            Debug.LogError("Fade image not assigned to FadeTransition!");
        }
    }

    // Fade to black only
    public IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade image not assigned to FadeTransition!");
            yield break;
        }

        // Ensure the fade image is active
        fadeImage.gameObject.SetActive(true);
        
        // Fade the alpha from 0 to 1
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        // Ensure its fully black
        color.a = 1f;
        fadeImage.color = color;
    }

    // Fade from black to clear
    public IEnumerator FadeToClear()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade image not assigned to FadeTransition!");
            yield break;
        }

        // Fade the alpha from 1 to 0
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeOutDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        // Ensure its fully transparent
        color.a = 0f;
        fadeImage.color = color;
    }
} 