using UnityEngine;
using System.Collections;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform defaultSpawn;

    [Header("Optional Movement Bounds")]
    public bool applyMovementBounds = false;
    public Vector2 minPosition = new Vector2(1, 1);
    public Vector2 maxPosition = new Vector2(9, 6);

    void Awake()
    {
        // Ensure the game initializer exists
        if (FindObjectOfType<GameInitializer>() == null)
        {
            GameObject initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
        }
    }

    void Start()
    {
        string spawnPointName = PlayerPrefs.GetString("SpawnPoint", "HouseExit");
        Debug.Log("SpawnPoint set to: " + spawnPointName);

        Transform spawnTransform = defaultSpawn;

        GameObject targetSpawn = GameObject.Find(spawnPointName);
        if (targetSpawn != null)
        {
            spawnTransform = targetSpawn.transform;
            Debug.Log("Found spawn point: " + spawnPointName);
        }
        else
        {
            Debug.LogWarning("No matching spawn point found. Using default.");
        }

        if (GameObject.FindWithTag("Player") == null)
        {
            GameObject player = Instantiate(playerPrefab, spawnTransform.position, Quaternion.identity);

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null && applyMovementBounds)
            {
                movement.useBounds = true;
                movement.minPosition = minPosition;
                movement.maxPosition = maxPosition;
                Debug.Log("Movement bounds set for player.");
            }
            
            // Check if in the FarmScene and initialize SoilTiller
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "FarmScene")
            {
                StartCoroutine(InitializeSoilTiller(player));
            }
        }

        PlayerPrefs.DeleteKey("SpawnPoint");
    }
    
    private IEnumerator InitializeSoilTiller(GameObject player)
    {
        // Wait briefly for scene to fully initialize
        yield return new WaitForSeconds(0.2f);
        
        // Find the HoeTool component in the player hierarchy
        SoilTiller[] soilTillers = player.GetComponentsInChildren<SoilTiller>(true);
        if (soilTillers != null && soilTillers.Length > 0)
        {
            foreach (SoilTiller tiller in soilTillers)
            {
                Debug.Log($"Found SoilTiller on {tiller.gameObject.name}, initializing references");
                tiller.InitializeReferences();
            }
        }
        else
        {
            Debug.LogWarning("No SoilTiller component found in player hierarchy");
        }
        
        // Double check after more time to ensure initialization happened
        yield return new WaitForSeconds(0.3f);
        
        // Find the SoilTiller again and verify its initialized
        SoilTiller[] tillers = player.GetComponentsInChildren<SoilTiller>(true);
        foreach (SoilTiller tiller in tillers)
        {
            if (!tiller.IsInitialized())
            {
                Debug.LogWarning($"SoilTiller on {tiller.gameObject.name} still not initialized, forcing reinitialization");
                tiller.ForceReinitialize();
            }
            else
            {
                Debug.Log($"SoilTiller on {tiller.gameObject.name} successfully initialized");
            }
        }
    }
}
