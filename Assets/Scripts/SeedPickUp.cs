using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedPickUp : MonoBehaviour
{
    public string seedName;
    public GameObject plantPrefab;

    private bool hasBeenPickedUp = false;
    private string uniqueId = "";
    private Scene currentScene;
    private void Awake()
    {
        currentScene = SceneManager.GetActiveScene();
        uniqueId = GenerateUniqueId();
        Debug.Log($"SeedPickUp initialized: {seedName} at {transform.position}, uniqueId: {uniqueId}, scene: {currentScene.name}");
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            // Generate a unique ID for this specific seed pickup
            CheckIfAlreadyPickedUp();
        }
    }

    private string GenerateUniqueId()
    {
        // Create a unique ID based on position and scene name
        Vector3 pos = transform.position;
        string scenePrefix = SceneManager.GetActiveScene().name.Substring(0, Mathf.Min(3, SceneManager.GetActiveScene().name.Length));
        return $"{scenePrefix}_{Mathf.RoundToInt(pos.x * 10)}_{Mathf.RoundToInt(pos.y * 10)}";
    }
    
    private void CheckIfAlreadyPickedUp()
    {
        // Check if this seed exists in the current inventory
        foreach (var item in GameManager.Instance.CurrentGameState.inventoryItems)
        {
            if (item.itemName == seedName)
            {
                Debug.Log($"Seed {seedName} is in inventory, but this doesn't guarantee this specific instance was picked up");
            }
        }
        
        // Also check GameManager seedPickups list to see if this specific seed was picked up
        Vector3 pos = transform.position;
        string posId = $"{Mathf.RoundToInt(pos.x * 10)}-{Mathf.RoundToInt(pos.y * 10)}";
        
        foreach (var pickup in GameManager.Instance.CurrentGameState.seedPickups)
        {
            // Check if positions match closely
            if (pickup.positionId == posId && pickup.isPickedUp)
            {
                Debug.Log($"FOUND PREVIOUSLY PICKED UP SEED: {seedName} at position: {posId}, destroying");
                Destroy(gameObject);
                return;
            }
        }
        
        // Register this seed with GameManager so it knows about all seeds in the scene
        GameManager.Instance.RegisterSeedInScene(seedName, transform.position);
        Debug.Log($"Registered seed in scene: {seedName} at {posId}, uniqueId: {uniqueId}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenPickedUp)
            return;
            
        if (other.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null)
            {
                hasBeenPickedUp = true;
                InventoryManager.Instance.AddItem(seedName, plantPrefab);
                
                // Record this seed as picked up in the GameManager
                if (GameManager.Instance != null)
                {
                    Debug.Log($"PICKUP EVENT: Seed {seedName} at position {transform.position}, uniqueId: {uniqueId}");
                    GameManager.Instance.RecordSeedPickup(seedName, transform.position);
                    // Force save to ensure the pickup is recorded permanently
                    GameManager.Instance.SaveGameState(); 
                }
                
                Destroy(gameObject);
            }
        }
    }
}
