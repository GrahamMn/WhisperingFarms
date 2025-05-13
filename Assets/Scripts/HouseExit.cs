using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HouseExit : MonoBehaviour
{
    public string targetScene = "FarmScene";
    public string spawnPointName = "HouseExit";
    
    public TextMeshProUGUI feedbackText;
    
    private bool isPlayerInRange = false;
    private float feedbackDisplayTimer = 0f;
    private const float FEEDBACK_DISPLAY_DURATION = 2f;

    void Update()
    {
        // Decrease feedback timer if active
        if (feedbackDisplayTimer > 0)
        {
            feedbackDisplayTimer -= Time.deltaTime;
            if (feedbackDisplayTimer <= 0 && feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }
        
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (SceneController.Instance != null)
            {
                // Check if can transition scenes
                if (SceneController.Instance.CanTransitionScene())
                {
                    SceneController.Instance.ChangeScene(targetScene, spawnPointName);
                }
                else
                {
                    // Display feedback when transition is on cooldown
                    DisplayFeedback("Please wait a moment...");
                }
            }
            else
            {
                // Fallback to original implementation
                PlayerPrefs.SetString("SpawnPoint", spawnPointName);
                PlayerPrefs.Save();
                SceneManager.LoadScene(targetScene);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            DisplayFeedback("Press E to exit house");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
                feedbackDisplayTimer = 0f;
            }
        }
    }
    
    private void DisplayFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
            feedbackDisplayTimer = FEEDBACK_DISPLAY_DURATION;
        }
        else
        {
            Debug.Log($"House exit: {message}");
        }
    }
}
