using UnityEngine;

public class SleepPoint : MonoBehaviour
{
    private bool isPlayerInRange = false;
    public GameObject sleepMenuCanvas; 

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed while near bed");

            if (sleepMenuCanvas != null)
            {
                Debug.Log("Activating sleep menu via reference");
                sleepMenuCanvas.SetActive(true);
            }
            else
            {
                Debug.LogWarning("sleepMenuCanvas reference is missing!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered bed trigger");
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left bed trigger");
            isPlayerInRange = false;
        }
    }
}
