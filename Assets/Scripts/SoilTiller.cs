using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class SoilTiller : MonoBehaviour
{
    public Tilemap GroundTilemap;
    public Tilemap soilTilemap;
    public Tilemap blockingTilemap;
    public TileBase soilTile;
    private bool hasInitializedReferences = false;
    void OnEnable()
    {
        // ensures assignments work even if the GameObject is enabled after Start
        if (!hasInitializedReferences)
        {
            StartCoroutine(InitializeWithDelay());
        }
    }

    void Start()
    {
        if (!hasInitializedReferences)
        {
            StartCoroutine(InitializeWithDelay());
        }
    }
    
    // Public method that can be called by any script that instantiates the player
    public void InitializeReferences()
    {
        if (!hasInitializedReferences)
        {
            EnsureReferences();
        }
    }
    
    // Wait a short time to ensure everything in the scene is loaded
    private IEnumerator InitializeWithDelay()
    {
        // Wait one frame to ensure all scene objects are loaded
        yield return null;
        
        // Wait a bit more to be sure
        yield return new WaitForSeconds(0.1f);
        
        // Now initialize references
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        bool madeChanges = false;
        
        // Don't try to find tilemaps if not in the farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "FarmScene") {
            // Assign 'Soil Tilemap' in FarmScene to soilTilemap
            soilTilemap = GameObject.Find("Soil Tilemap").GetComponent<Tilemap>();
            // Assign 'Ground Tilemap' in FarmScene to GroundTilemap
            GroundTilemap = GameObject.Find("Ground Tilemap").GetComponent<Tilemap>();
            // Assign 'Stone Tilemap' in FarmScene to blockingTilemap
            blockingTilemap = GameObject.Find("Stone Tilemap").GetComponent<Tilemap>();
        }
        else {
            
            return;
        }
        
        hasInitializedReferences = true;
        
        if (madeChanges)
        {
            Debug.Log($"SoilTiller: Finished setting up references in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }
    }
    

    public void TillSoil(Vector3Int position)
    {
        // Make sure references are initialized
        if (!hasInitializedReferences)
        {
            EnsureReferences();
        }
        
        // Dont try to till soil if not in farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
        {
            Debug.LogWarning("SoilTiller: Cannot till soil - not in FarmScene");
            return;
        }
        
        // Check required references
        if (soilTilemap == null)
        {
            Debug.LogError("Cannot till soil: soilTilemap is null");
            return;
        }
        
        if (GroundTilemap == null)
        {
            Debug.LogError("Cannot till soil: GroundTilemap is null");
            return;
        }

        if (GroundTilemap.GetTile(position) == null)
        {
            Debug.Log("Tile is not valid ground");
            return;
        }

        if (blockingTilemap != null && blockingTilemap.GetTile(position) != null)
        {
            Debug.Log("Cannot till — blocked by stone or patio");
            return;
        }

        if (soilTilemap.GetTile(position) == null)
        {   
            soilTilemap.SetTile(position, soilTile);
            Debug.Log($"Tilled soil at: {position}");
        }
        else
        {
            Debug.Log("Tile is already tilled");
        }
    }

    public void Till()
    {
        // If we are not in the farm scene don't do anything
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
        {
            Debug.LogWarning("SoilTiller: Not in FarmScene, tilling disabled");
            return;
        }
        
        // Ensure references are set up
        if (!hasInitializedReferences || soilTilemap == null || GroundTilemap == null)
        {
            EnsureReferences();
            
            // If still null after trying to find references cant proceed
            if (soilTilemap == null || GroundTilemap == null)
            {
                Debug.LogError("SoilTiller: Cannot till - required tilemaps not found");
                return;
            }
        }
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("SoilTiller: Player not found");
            return;
        }
        
        Transform player = playerObj.transform;
        Vector3 facingDir = GetPlayerFacingDirection();
        Vector3 targetPos = player.position + facingDir;
        Vector3Int gridPos = soilTilemap.WorldToCell(targetPos);

        TillSoil(gridPos);
    }

    private Vector3 GetPlayerFacingDirection()
    {
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (moveInput.y > 0) return Vector3.up;
        if (moveInput.x > 0) return Vector3.right;
        if (moveInput.y < 0) return Vector3.down;
        if (moveInput.x < 0) return Vector3.left;

        return Vector3.down; 
    }
    
    // Public methods to check if the SoilTiller is initialized
    public bool IsInitialized()
    {
        return hasInitializedReferences && soilTilemap != null && GroundTilemap != null && soilTile != null;
    }
    
    public void ForceReinitialize()
    {
        hasInitializedReferences = false;
        EnsureReferences();
    }
}
