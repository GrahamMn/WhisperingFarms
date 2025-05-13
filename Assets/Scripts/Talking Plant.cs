using UnityEngine;

public class TalkingPlant : MonoBehaviour
{
    public Sprite[] growthStages;
    public float timeBetweenStages = 5f;
    public GameObject speechBubblePrefab;

    private SpriteRenderer spriteRenderer;
    public int currentStage = 0;
    private float timer = 0f;
    public bool isWatered = false;

    private SpeechBubbleController speechBubble;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize sprite renderer
        UpdateVisuals();

        if (speechBubblePrefab != null)
        {
            GameObject bubbleObj = Instantiate(speechBubblePrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
            speechBubble = bubbleObj.GetComponent<SpeechBubbleController>();
            bubbleObj.SetActive(false);
        }
    }

    void Update()
    {
        if (growthStages.Length == 0 || currentStage >= growthStages.Length - 1 || !isWatered)
            return;

        timer += Time.deltaTime;

        if (timer >= timeBetweenStages)
        {
            currentStage++;
            UpdateVisuals();
            timer = 0f;
            isWatered = false;

            if (currentStage >= growthStages.Length - 1)
            {
                HarvestablePlant harvest = GetComponent<HarvestablePlant>();
                if (harvest != null)
                    harvest.isFullyGrown = true;
            }

            if (speechBubble != null)
            {
                speechBubble.gameObject.SetActive(true);
                speechBubble.ShowText("I'm growing!", 2f);
            }
        }
    }

    public void UpdateVisuals()
    {
        // Make sure we have a sprite renderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"No SpriteRenderer found on {gameObject.name}");
                return;
            }
        }

        // Make sure we have growth stages
        if (growthStages == null || growthStages.Length == 0)
        {
            Debug.LogWarning($"No growth stages defined for {gameObject.name}");
            return;
        }

        // Validate current stage
        if (currentStage < 0)
            currentStage = 0;
        
        if (currentStage >= growthStages.Length)
            currentStage = growthStages.Length - 1;

        // Apply the sprite for the current stage
        if (growthStages[currentStage] != null)
        {
            spriteRenderer.sprite = growthStages[currentStage];
            Debug.Log($"{gameObject.name} visual updated to stage {currentStage}");
        }
        else
        {
            Debug.LogError($"Sprite at index {currentStage} is null for {gameObject.name}");
        }
    }

    public void ReceiveWater()
    {
        isWatered = true;

        if (speechBubble != null)
        {
            speechBubble.gameObject.SetActive(true);
            speechBubble.ShowText("Thanks for the water!", 2f);
        }

        Debug.Log($"{name} received water!");
    }

    public void Water()
    {
        Debug.Log("Plant was watered!");
    }
}
