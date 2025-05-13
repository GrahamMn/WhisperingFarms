using UnityEngine;

public class HarvestablePlant : MonoBehaviour
{
    [Tooltip("Name of the harvested item (e.g., 'Tomato', 'Pumpkin')")]
    public string harvestItemName = "Radish";  // Default value, will be overridden per plant

    public bool isFullyGrown = false;
    public bool isHarvested = false;

    private void OnEnable()
    {
        // When the object is enabled check if it should be hidden
        if (isHarvested)
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
                Debug.Log($"{gameObject.name} is harvested, hiding sprite");
            }
        }
    }

    public void Harvest()
    {
        if (!isFullyGrown)
        {
            Debug.Log("Cannot harvest — not fully grown.");
            return;
        }

        if (isHarvested)
        {
            Debug.Log("Already harvested.");
            return;
        }

        Debug.Log("Harvested: " + harvestItemName);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddHarvest(harvestItemName);
        }
        else
        {
            Debug.LogWarning(" InventoryManager.Instance is null.");
        }

        // If this is a talking plant, record it in the GameManager
        if (gameObject.name.Contains("Talking") && GameManager.Instance != null)
        {
            GameManager.Instance.RecordTalkingPlantHarvested(transform.position);
            Debug.Log($"Recorded talking plant harvest at {transform.position}");
        }

        // Instead of just hiding the plant destroy the GameObject
        Destroy(gameObject);
    }
}
