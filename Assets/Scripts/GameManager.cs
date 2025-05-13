using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Serializable]
    public class SerializedInventoryItem
    {
        public string itemName;
        public int count;
    }

    [Serializable]
    public class SerializedTilledSoil
    {
        public int x;
        public int y;
        public int z;

        public SerializedTilledSoil(Vector3Int position)
        {
            x = position.x;
            y = position.y;
            z = position.z;
            Debug.Log($"SerializedTilledSoil: Created new soil position at ({x}, {y}, {z})");
        }

        public Vector3Int ToVector3Int()
        {
            Vector3Int result = new Vector3Int(x, y, z);
            Debug.Log($"SerializedTilledSoil: Converting to Vector3Int ({x}, {y}, {z})");
            return result;
        }
        
        public override string ToString()
        {
            return $"Soil({x}, {y}, {z})";
        }
    }

    [Serializable]
    public class SerializedPlant
    {
        public string plantType; // The name of the plant prefab
        public Vector3 position;
        public int growthStage;
        public bool isWatered;
        public bool isFullyGrown;
        public bool isHarvested;
        
        // Add a uniqueidentifier to help match the plants
        public string positionId => $"{Mathf.RoundToInt(position.x * 10)}-{Mathf.RoundToInt(position.y * 10)}";
    }

    [Serializable]
    public class SerializedSeedPickup
    {
        public string seedName;
        public Vector3 position;
        public bool isPickedUp;
        
        // Add a unique identifier to help match seeds
        public string positionId => $"{Mathf.RoundToInt(position.x * 10)}-{Mathf.RoundToInt(position.y * 10)}";
    }

    // Dictionary to hold references to the plant prefabsbetween scenes
    private Dictionary<string, GameObject> seedPrefabCache = new Dictionary<string, GameObject>();

    // Store a list of the picked up seeds across all scenes
    private HashSet<string> allPickedUpSeedPositionIds = new HashSet<string>();
    
    // Store a list of all harvested talking plant positions
    private HashSet<string> harvestedTalkingPlantPositionIds = new HashSet<string>();

    [Serializable]
    public class GameState
    {
        public int money;
        public float timeOfDay;
        public List<SerializedInventoryItem> inventoryItems = new List<SerializedInventoryItem>();
        public List<SerializedInventoryItem> harvestedItems = new List<SerializedInventoryItem>();
        public List<SerializedTilledSoil> tilledSoilPositions = new List<SerializedTilledSoil>();
        public List<SerializedPlant> plants = new List<SerializedPlant>();
        public List<SerializedSeedPickup> seedPickups = new List<SerializedSeedPickup>();
        public string selectedSeed = "";
        public string selectedTool = "";
        public bool hasWateringCan = true;
        public bool hasHoeTool = true;
        public List<string> harvestedTalkingPlantPositionIds = new List<string>();
        
        // Constructor to ensure all lists are properly initialized
        public GameState()
        {
            inventoryItems = new List<SerializedInventoryItem>();
            harvestedItems = new List<SerializedInventoryItem>();
            tilledSoilPositions = new List<SerializedTilledSoil>();
            plants = new List<SerializedPlant>();
            seedPickups = new List<SerializedSeedPickup>();
            harvestedTalkingPlantPositionIds = new List<string>();
            
            Debug.Log("GameState: Created new instance with initialized lists");
        }
    }

    public GameState CurrentGameState { get; private set; } = new GameState();
    
    private bool isFirstSceneLoad = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager created as singleton");
            
            // Initialize the master list of picked up seeds
            allPickedUpSeedPositionIds = new HashSet<string>();
            
            // Initialize the list of harvested talking plants
            harvestedTalkingPlantPositionIds = new HashSet<string>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SaveGameState()
    {
        Debug.Log("SaveGameState: Saving all game state...");
        
        SaveMoneyState();
        SaveInventoryState();
        SaveTimeState();
        SavePlantState();
        SaveSeedPickupState();
        SaveHarvestedTalkingPlantsState();
        SaveTilledSoilState();
        
        // Save the entire game state to PlayerPrefs
        string gameStateJson = JsonUtility.ToJson(CurrentGameState);
        PlayerPrefs.SetString("SavedGameState", gameStateJson);
        PlayerPrefs.Save();
        
        // Log the count of tilled soil positions in the serialized data
        Debug.Log($"SaveGameState: Saved game state to PlayerPrefs with {CurrentGameState.tilledSoilPositions.Count} tilled soil positions");
        
        // Validate the saved data 
        string savedJson = PlayerPrefs.GetString("SavedGameState");
        if (!string.IsNullOrEmpty(savedJson))
        {
            try
            {
                GameState validationState = JsonUtility.FromJson<GameState>(savedJson);
                Debug.Log($"SaveGameState: Validated saved data - contains {validationState.tilledSoilPositions.Count} tilled soil positions");
                
                // Print out the first few positions for debugging
                int debugCount = Mathf.Min(validationState.tilledSoilPositions.Count, 5);
                for (int i = 0; i < debugCount; i++)
                {
                    SerializedTilledSoil pos = validationState.tilledSoilPositions[i];
                    Debug.Log($"SaveGameState: Validated tilled soil [{i}] = ({pos.x}, {pos.y}, {pos.z})");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveGameState: Error validating saved data: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("SaveGameState: Failed to save game state - PlayerPrefs value is empty after saving!");
        }
    }

    public void LoadGameState()
    {
        Debug.Log("LoadGameState: Beginning to load all game state...");
        
        if (isFirstSceneLoad)
        {
            Debug.Log("LoadGameState: First scene load, not restoring state yet");
            isFirstSceneLoad = false;
            return;
        }

        // First try to load the complete game state from PlayerPrefs
        if (PlayerPrefs.HasKey("SavedGameState"))
        {
            string savedJson = PlayerPrefs.GetString("SavedGameState");
            if (!string.IsNullOrEmpty(savedJson))
            {
                try
                {
                    CurrentGameState = JsonUtility.FromJson<GameState>(savedJson);
                    Debug.Log($"LoadGameState: Successfully loaded game state from PlayerPrefs with {CurrentGameState.tilledSoilPositions.Count} tilled soil positions");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"LoadGameState: Error loading game state from PlayerPrefs: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("LoadGameState: PlayerPrefs key exists but value is empty");
            }
        }
        else
        {
            Debug.LogWarning("LoadGameState: No saved game state found in PlayerPrefs");
        }

        LoadMoneyState();
        LoadInventoryState();
        LoadTimeState();
        
        Debug.Log("LoadGameState: About to load tilled soil state...");
        LoadTilledSoilState();
        
        LoadPlantState();
        LoadSeedPickupState();
        LoadHarvestedTalkingPlantsState();
        
        Debug.Log("LoadGameState: All game state loaded successfully");
    }

    private void SaveMoneyState()
    {
        MoneyManager moneyManager = FindObjectOfType<MoneyManager>();
        if (moneyManager != null)
        {
            CurrentGameState.money = moneyManager.money;
            Debug.Log($"Saved money: {CurrentGameState.money}");
        }
    }

    private void LoadMoneyState()
    {
        MoneyManager moneyManager = FindObjectOfType<MoneyManager>();
        if (moneyManager != null)
        {
            moneyManager.money = CurrentGameState.money;
            Debug.Log($"Loaded money: {CurrentGameState.money}");
        }
    }

    private void SaveInventoryState()
    {
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            // Save seed prefab references
            if (inventoryManager.seedToPlantPrefabs != null && inventoryManager.seedToPlantPrefabs.Count > 0)
            {
                seedPrefabCache.Clear();
                foreach (var kvp in inventoryManager.seedToPlantPrefabs)
                {
                    seedPrefabCache[kvp.Key] = kvp.Value;
                    Debug.Log($"Cached seed prefab for: {kvp.Key}");
                }
            }

            // Mark tools as present
            CurrentGameState.hasWateringCan = true;
            CurrentGameState.hasHoeTool = true;

            // Clear existing inventory items
            CurrentGameState.inventoryItems.Clear();
            CurrentGameState.harvestedItems.Clear();
            
            // Save the selected items
            CurrentGameState.selectedSeed = inventoryManager.GetSelectedSeed();
            CurrentGameState.selectedTool = inventoryManager.GetSelectedTool();
            
            // Get the private itemCounts dictionary using reflection
            var itemCountsField = typeof(InventoryManager).GetField("itemCounts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (itemCountsField != null)
            {
                var itemCounts = itemCountsField.GetValue(inventoryManager) as Dictionary<string, int>;
                if (itemCounts != null)
                {
                    foreach (var item in itemCounts)
                    {
                        CurrentGameState.inventoryItems.Add(new SerializedInventoryItem
                        {
                            itemName = item.Key,
                            count = item.Value
                        });
                    }
                }
            }
            
            // Get the private harvestCounts dictionary using reflection
            var harvestCountsField = typeof(InventoryManager).GetField("harvestCounts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (harvestCountsField != null)
            {
                var harvestCounts = harvestCountsField.GetValue(inventoryManager) as Dictionary<string, int>;
                if (harvestCounts != null)
                {
                    foreach (var item in harvestCounts)
                    {
                        CurrentGameState.harvestedItems.Add(new SerializedInventoryItem
                        {
                            itemName = item.Key,
                            count = item.Value
                        });
                    }
                }
            }
            
            Debug.Log($"Saved inventory with {CurrentGameState.inventoryItems.Count} items and {CurrentGameState.harvestedItems.Count} harvests");
        }
    }

    private void LoadInventoryState()
    {
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();

        // If not in FarmScene, skip loading inventory
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
        {
            Debug.Log("LoadInventoryState: Not in FarmScene, skipping inventory load");
            return;
        }
        if (inventoryManager == null)
        {
            Debug.LogError("Cannot load inventory state: InventoryManager not found!");
            return;
        }

        Debug.Log("Loading inventory state...");

        // Forcibly clear existing inventory
        var clearInventoryMethod = typeof(InventoryManager).GetMethod("ClearInventory", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (clearInventoryMethod != null)
        {
            clearInventoryMethod.Invoke(inventoryManager, null);
            Debug.Log("Cleared existing inventory");
        }
        
        // Make sure tools are properly initialized first
        inventoryManager.RestoreTools();
        Debug.Log("Tools restored");

        // Restore seed prefabs from cache
        if (seedPrefabCache.Count > 0)
        {
            foreach (var kvp in seedPrefabCache)
            {
                inventoryManager.seedToPlantPrefabs[kvp.Key] = kvp.Value;
                Debug.Log($"Restored seed prefab for: {kvp.Key}");
            }
        }
        
        // Restore inventory items
        foreach (var item in CurrentGameState.inventoryItems)
        {
            for (int i = 0; i < item.count; i++)
            {
                // Skip tools as they are handled separately
                if (item.itemName == "Watering Can" || item.itemName == "Hoe Tool")
                    continue;
                    
                // Pass the cached prefab if it exists
                if (seedPrefabCache.TryGetValue(item.itemName, out GameObject prefab))
                {
                    inventoryManager.AddItem(item.itemName, prefab);
                }
                else
                {
                    inventoryManager.AddItem(item.itemName);
                }
            }
        }
        
        Debug.Log($"Restored {CurrentGameState.inventoryItems.Count} inventory items");
        
        // Restore harvested items using the dedicated method
        int harvestCount = 0;
        foreach (var harvest in CurrentGameState.harvestedItems)
        {
            inventoryManager.RestoreHarvest(harvest.itemName, harvest.count);
            harvestCount += harvest.count;
        }
        
        Debug.Log($"Restored {CurrentGameState.harvestedItems.Count} harvested item types ({harvestCount} total items)");
        
        // Restore selected items
        if (!string.IsNullOrEmpty(CurrentGameState.selectedSeed))
        {
            inventoryManager.SelectSeed(CurrentGameState.selectedSeed);
            Debug.Log($"Selected seed: {CurrentGameState.selectedSeed}");
        }
        
        if (!string.IsNullOrEmpty(CurrentGameState.selectedTool))
        {
            inventoryManager.SelectTool(CurrentGameState.selectedTool);
            Debug.Log($"Selected tool: {CurrentGameState.selectedTool}");
        }
        
        Debug.Log("Inventory state loaded successfully");
    }

    private void SaveTimeState()
    {
        TimeManager timeManager = FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            var timeField = typeof(TimeManager).GetField("time", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (timeField != null)
            {
                CurrentGameState.timeOfDay = (float)timeField.GetValue(timeManager);
                Debug.Log($"Saved time of day: {CurrentGameState.timeOfDay}");
            }
        }
    }

    private void LoadTimeState()
    {
        TimeManager timeManager = FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            var timeField = typeof(TimeManager).GetField("time", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (timeField != null)
            {
                timeField.SetValue(timeManager, CurrentGameState.timeOfDay);
                Debug.Log($"Loaded time of day: {CurrentGameState.timeOfDay}");
            }
        }
    }

    private void SaveTilledSoilState()
    {
        // Only save the tilled soil state if we're in the farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
        {
            Debug.Log("SaveTilledSoilState: Not in FarmScene, skipping");
            return;
        }
            
        // Ensure list is initialized
        if (CurrentGameState.tilledSoilPositions == null)
        {
            CurrentGameState.tilledSoilPositions = new List<SerializedTilledSoil>();
        }
        else
        {
            // Clear previous state
            CurrentGameState.tilledSoilPositions.Clear();
        }
        
        // First try to find an existing SoilTiller
        SoilTiller soilTiller = FindObjectOfType<SoilTiller>();
        bool createdTemporarySoilTiller = false;
        
        // If no SoilTiller found, create a temporary one
        if (soilTiller == null)
        {
            Debug.Log("SaveTilledSoilState: No SoilTiller found, creating a temporary one");
            
            // Create a temporary GameObject to hold the SoilTiller
            GameObject tempObject = new GameObject("TempSoilTiller");
            soilTiller = tempObject.AddComponent<SoilTiller>();
            createdTemporarySoilTiller = true;
            
            // Initialize the SoilTiller
            soilTiller.InitializeReferences();
            
            // Wait a moment for initialization to complete
            // Since we can't yield here, we'll use the direct approach
        }
        
        // Try to use the SoilTiller's tilemap if itis valid
        if (soilTiller != null && soilTiller.soilTilemap != null)
        {
            SaveTilledSoilFromTilemap(soilTiller.soilTilemap);
            
            // If we created a temporary SoilTiller, destroy it
            if (createdTemporarySoilTiller)
            {
                Destroy(soilTiller.gameObject);
            }
            return;
        }
        
        // If SoilTiller doesn't have a valid tilemap, try to find Soil Tilemap directly
        Debug.Log("SaveTilledSoilState: SoilTiller doesn't have valid tilemap, looking for Soil Tilemap directly");
            
        // Try to find Soil Tilemap directly
        GameObject soilTilemapObj = GameObject.Find("Soil Tilemap");
        if (soilTilemapObj != null)
        {
            UnityEngine.Tilemaps.Tilemap directTilemap = soilTilemapObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (directTilemap != null)
            {
                Debug.Log("SaveTilledSoilState: Found Soil Tilemap directly, scanning for tilled tiles");
                SaveTilledSoilFromTilemap(directTilemap);
                
                // If we created a temporary SoilTiller, destroy it
                if (createdTemporarySoilTiller)
                {
                    Destroy(soilTiller.gameObject);
                }
                return;
            }
        }
        
        // Try lowercase version
        soilTilemapObj = GameObject.Find("soil tilemap");
        if (soilTilemapObj != null)
        {
            UnityEngine.Tilemaps.Tilemap directTilemap = soilTilemapObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (directTilemap != null)
            {
                Debug.Log("SaveTilledSoilState: Found lowercase soil tilemap directly, scanning for tilled tiles");
                SaveTilledSoilFromTilemap(directTilemap);
                
                // If we created a temporary SoilTiller, destroy it
                if (createdTemporarySoilTiller)
                {
                    Destroy(soilTiller.gameObject);
                }
                return;
            }
        }
        
        // If we created a temporary SoilTiller, destroy it
        if (createdTemporarySoilTiller)
        {
            Destroy(soilTiller.gameObject);
        }
        
        Debug.LogError("SaveTilledSoilState: Could not find SoilTiller or Soil Tilemap");
    }
    
    private void SaveTilledSoilFromTilemap(Tilemap soilTilemap)
    {
        if (soilTilemap == null)
        {
            Debug.LogError("SaveTilledSoilFromTilemap: Tilemap is null");
            return;
        }
        
        BoundsInt bounds = soilTilemap.cellBounds;
        Debug.Log($"SaveTilledSoilState: Scanning tilemap \"{soilTilemap.name}\" in bounds {bounds.min} to {bounds.max}");
        
        int tilesFound = 0;
        
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (soilTilemap.GetTile(pos) != null)
                {
                    CurrentGameState.tilledSoilPositions.Add(new SerializedTilledSoil(pos));
                    tilesFound++;
                    
                    // Don't spam log for each tile, just log a few for debugging
                    if (tilesFound <= 5)
                    {
                        Debug.Log($"SaveTilledSoilState: Found tilled soil at ({x}, {y}, 0)");
                    }
                }
            }
        }
        
        Debug.Log($"SaveTilledSoilState: Found {tilesFound} tilled soil positions in \"{soilTilemap.name}\"");
        
        if (tilesFound == 0)
        {
            Debug.LogWarning("SaveTilledSoilState: No tilled soil found in the tilemap. If you expect tilled soil, check that it's using the correct tilemap.");
        }
    }

    private void LoadTilledSoilState()
    {
        // Only load the tilled soil state if we're in the farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
        {
            Debug.Log("LoadTilledSoilState: Not in FarmScene, skipping");
            return;
        }
        
        // Ensure tilledSoilPositions is not null
        if (CurrentGameState.tilledSoilPositions == null)
        {
            Debug.LogError("LoadTilledSoilState: tilledSoilPositions is null! Initializing empty list.");
            CurrentGameState.tilledSoilPositions = new List<SerializedTilledSoil>();
            return; 
        }
        
        // Skip if there are no tilled soil positions to restore
        if (CurrentGameState.tilledSoilPositions.Count == 0)
        {
            Debug.Log("LoadTilledSoilState: No tilled soil positions to restore");
            return;
        }
        
        // Use direct approach to load immediately rather than waiting
        LoadTilledSoilStateImmediate();
    }
    
    private void LoadTilledSoilStateImmediate()
    {
        Debug.Log("LoadTilledSoilStateImmediate: Starting immediate load");
        
        // Try to find player-based SoilTiller first
        GameObject player = GameObject.FindWithTag("Player");
        SoilTiller soilTiller = null;
        bool createdTempSoilTiller = false;
        
        if (player != null)
        {
            SoilTiller[] tillers = player.GetComponentsInChildren<SoilTiller>(true);
            if (tillers != null && tillers.Length > 0)
            {
                soilTiller = tillers[0];
                Debug.Log($"LoadTilledSoilStateImmediate: Found SoilTiller on player's {soilTiller.gameObject.name}");
                
                // Ensure it's initialized 
                if (!soilTiller.IsInitialized())
                {
                    Debug.Log("LoadTilledSoilStateImmediate: SoilTiller not initialized, initializing it");
                    soilTiller.InitializeReferences();
                }
            }
        }
        
        // If not found on player, try to find it in the scene
        if (soilTiller == null)
        {
            soilTiller = FindObjectOfType<SoilTiller>();
            if (soilTiller != null)
            {
                Debug.Log("LoadTilledSoilStateImmediate: Found SoilTiller in scene");
                
                // Ensure it's initialized
                if (!soilTiller.IsInitialized())
                {
                    Debug.Log("LoadTilledSoilStateImmediate: SoilTiller not initialized, initializing it");
                    soilTiller.InitializeReferences();
                }
            }
            else
            {
                // Create a temporary SoilTiller
                Debug.Log("LoadTilledSoilStateImmediate: No SoilTiller found, creating a temporary one");
                GameObject tempObj = new GameObject("TempSoilTiller");
                soilTiller = tempObj.AddComponent<SoilTiller>();
                createdTempSoilTiller = true;
                
                // Initialize the temporary SoilTiller
                soilTiller.InitializeReferences();
            }
        }
        
        // If we have a valid SoilTiller by this point, use it
        if (soilTiller != null && soilTiller.soilTilemap != null && soilTiller.soilTile != null)
        {
            Debug.Log($"LoadTilledSoilStateImmediate: Using SoilTiller to restore {CurrentGameState.tilledSoilPositions.Count} tilled soil positions");
            ApplyTilledSoilToTilemap(soilTiller.soilTilemap, soilTiller.soilTile);
            
            // Clean up temporary object if created
            if (createdTempSoilTiller)
            {
                Destroy(soilTiller.gameObject);
            }
            return;
        }
        
        // If SoilTiller doesn't have valid references, try direct tilemap access
        Debug.Log("LoadTilledSoilStateImmediate: SoilTiller doesn't have valid references, trying direct tilemap access");
        
        // Look for Soil Tilemap directly
        GameObject soilTilemapObj = GameObject.Find("Soil Tilemap");
        if (soilTilemapObj == null)
        {
            soilTilemapObj = GameObject.Find("soil tilemap"); 
        }
        
        if (soilTilemapObj != null)
        {
            UnityEngine.Tilemaps.Tilemap soilTilemap = soilTilemapObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (soilTilemap != null)
            {
                Debug.Log("LoadTilledSoilStateImmediate: Found Soil Tilemap directly");
                ApplyTilledSoilToTilemap(soilTilemap);
                
                // Clean up temporary object if created
                if (createdTempSoilTiller)
                {
                    Destroy(soilTiller.gameObject);
                }
                return;
            }
        }
        
        // Clean up temporary object if created
        if (createdTempSoilTiller)
        {
            Destroy(soilTiller.gameObject);
        }
        
        Debug.LogError("LoadTilledSoilStateImmediate: Could not find or create valid SoilTiller or Soil Tilemap");
    }
    
    private void ApplyTilledSoilToTilemap(UnityEngine.Tilemaps.Tilemap soilTilemap, UnityEngine.Tilemaps.TileBase soilTile = null)
    {
        
        // Clear existing tilled soil
        soilTilemap.ClearAllTiles();
        Debug.Log("ApplyTilledSoilToTilemap: Cleared existing tilled soil");
        
        Debug.Log($"ApplyTilledSoilToTilemap: Attempting to restore {CurrentGameState.tilledSoilPositions.Count} tilled soil positions");
        
        // Track how many positions were successfully restored
        int successfulRestores = 0;
        
        // Restore tilled soil positions
        foreach (var position in CurrentGameState.tilledSoilPositions)
        {
            if (position == null)
            {
                Debug.LogError("ApplyTilledSoilToTilemap: Found null position in tilledSoilPositions list!");
                continue;
            }
            
            Vector3Int pos = position.ToVector3Int();
            soilTilemap.SetTile(pos, soilTile);
            successfulRestores++;
        }
        
        Debug.Log($"ApplyTilledSoilToTilemap: Successfully restored {successfulRestores} of {CurrentGameState.tilledSoilPositions.Count} tilled soil positions");

        // Force the tilemap to refresh its visuals
        soilTilemap.RefreshAllTiles();
    }
    
    private System.Collections.IEnumerator LoadTilledSoilStateWhenReady()
    {
        Debug.Log("LoadTilledSoilStateWhenReady: Waiting for scene to fully initialize");
        
        // Wait for scene to be fully loaded
        yield return new WaitForSeconds(0.5f);
        
        // Try to find SoilTiller through the player first (since it's attached to the HoeTool)
        GameObject player = GameObject.FindWithTag("Player");
        SoilTiller soilTiller = null;
        
        if (player != null)
        {
            SoilTiller[] tillers = player.GetComponentsInChildren<SoilTiller>(true);
            if (tillers != null && tillers.Length > 0)
            {
                soilTiller = tillers[0];
                Debug.Log($"LoadTilledSoilStateWhenReady: Found SoilTiller on player's {soilTiller.gameObject.name}");
                
                // Ensure it's initialized 
                if (!soilTiller.IsInitialized())
                {
                    Debug.Log("LoadTilledSoilStateWhenReady: SoilTiller not initialized, initializing it");
                    soilTiller.InitializeReferences();
                    
                    // Wait a moment for initialization
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        
        // If not found on player, try to find it in the scene
        if (soilTiller == null)
        {
            soilTiller = FindObjectOfType<SoilTiller>();
            if (soilTiller != null)
            {
                Debug.Log("LoadTilledSoilStateWhenReady: Found SoilTiller in scene");
                
                // Ensure it's initialized
                if (!soilTiller.IsInitialized())
                {
                    Debug.Log("LoadTilledSoilStateWhenReady: SoilTiller not initialized, initializing it");
                    soilTiller.InitializeReferences();
                    
                    // Wait a moment for initialization
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        
        // If still not found or initialized, try to look for Soil Tilemap directly
        if (soilTiller == null || !soilTiller.IsInitialized())
        {
            Debug.LogWarning("LoadTilledSoilStateWhenReady: No properly initialized SoilTiller found, looking for Soil Tilemap directly");
            
            GameObject soilTilemapObj = GameObject.Find("Soil Tilemap");
            if (soilTilemapObj == null)
            {
                soilTilemapObj = GameObject.Find("soil tilemap"); 
            }
            
            if (soilTilemapObj != null)
            {
                UnityEngine.Tilemaps.Tilemap soilTilemap = soilTilemapObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                if (soilTilemap != null)
                {
                    Debug.Log("LoadTilledSoilStateWhenReady: Found Soil Tilemap directly");
                    ApplyTilledSoilToTilemap(soilTilemap);
                    yield break; 
                }
            }
            
            Debug.LogError("LoadTilledSoilStateWhenReady: Could not find Soil Tilemap after multiple attempts");
            yield break; 
        }
        
        // If we have a valid SoilTiller by this point, use it
        if (soilTiller != null && soilTiller.soilTilemap != null && soilTiller.soilTile != null)
        {
            Debug.Log($"LoadTilledSoilStateWhenReady: Using SoilTiller to restore {CurrentGameState.tilledSoilPositions.Count} tilled soil positions");
            ApplyTilledSoilToTilemap(soilTiller.soilTilemap, soilTiller.soilTile);
        }
        else
        {
            Debug.LogError("LoadTilledSoilStateWhenReady: Failed to find valid SoilTiller or Soil Tilemap");
        }
    }

    private void SavePlantState()
    {
        // Only save plants if we're in the farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
            return;
        
        CurrentGameState.plants.Clear();
        
        // Find all plants in the scene
        TalkingPlant[] plants = FindObjectsOfType<TalkingPlant>();
        if (plants != null && plants.Length > 0)
        {
            foreach (var plant in plants)
            {
                // Skip null plants
                if (plant == null || plant.gameObject == null)
                    continue;
                
                // Get the plant prefab name from the GameObject name
                string plantType = plant.gameObject.name;
                
                // Keep the original name for special Talking plants
                bool isTalkingPlant = plantType.Contains("Talking");
                
                // Remove Clone if it exists
                if (plantType.EndsWith("(Clone)"))
                {
                    plantType = plantType.Substring(0, plantType.Length - 7);
                }
                
                // Handle any pre-set plants that might have numeric suffixes
                if (!isTalkingPlant) 
                {
                    int parenthesisIndex = plantType.LastIndexOf(" (");
                    if (parenthesisIndex > 0 && plantType[plantType.Length - 1] == ')')
                    {
                        // Try to parse the content between parentheses as a number
                        string potentialNumber = plantType.Substring(parenthesisIndex + 2, plantType.Length - parenthesisIndex - 3);
                        if (int.TryParse(potentialNumber, out int _))
                        {
                            plantType = plantType.Substring(0, parenthesisIndex);
                        }
                    }
                }
                
                // Create serialized plant data
                SerializedPlant plantData = new SerializedPlant
                {
                    plantType = plantType,
                    position = plant.transform.position,
                    growthStage = plant.currentStage,
                    isWatered = plant.isWatered,
                    isFullyGrown = false,
                    isHarvested = false
                };
                
                // Check if the plant is fully grown
                HarvestablePlant harvestable = plant.GetComponent<HarvestablePlant>();
                if (harvestable != null)
                {
                    plantData.isFullyGrown = harvestable.isFullyGrown;
                }
                
                CurrentGameState.plants.Add(plantData);
                Debug.Log($"Saved plant: {plantType} at position {plantData.position} ({plantData.positionId}), stage {plantData.growthStage}, watered: {plantData.isWatered}, fully grown: {plantData.isFullyGrown}");
            }
            
            Debug.Log($"Saved {CurrentGameState.plants.Count} plants");
        }
    }
    
    private void LoadPlantState()
    {
        // Only load plants if we're in the farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "FarmScene")
            return;
            
        // Get a list of all existing plants before destroying them
        Dictionary<string, GameObject> existingPlants = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> existingTalkingPlants = new Dictionary<string, GameObject>();
        Dictionary<string, TalkingPlant> talkingPlantComponents = new Dictionary<string, TalkingPlant>();
        
        TalkingPlant[] plantComponents = FindObjectsOfType<TalkingPlant>();
        
        foreach (var plant in plantComponents)
        {
            if (plant != null && plant.gameObject != null)
            {
                // Create a position-based ID for this plant
                Vector3 pos = plant.transform.position;
                string posId = $"{Mathf.RoundToInt(pos.x * 10)}-{Mathf.RoundToInt(pos.y * 10)}";
                
                // Check if it's a talking plant
                string name = plant.gameObject.name;
                if (name.Contains("Talking"))
                {
                    // Check if this talking plant was harvested
                    if (harvestedTalkingPlantPositionIds.Contains(posId))
                    {
                        Debug.Log($"Destroying harvested talking plant: {name} at position {posId}");
                        Destroy(plant.gameObject);
                        continue;
                    }
                    
                    existingTalkingPlants[name] = plant.gameObject;
                    talkingPlantComponents[name] = plant;
                    Debug.Log($"Found existing talking plant: {name} at position ID: {posId}");
                }
                else
                {
                    // Add to dictionary with position ID as key
                    existingPlants[posId] = plant.gameObject;
                    Debug.Log($"Found existing plant: {name} at position ID: {posId}");
                }
            }
        }
        
        // If we have no plants to restore, don't destroy the existing ones
        if (CurrentGameState.plants.Count == 0)
        {
            Debug.Log("No plants to restore from save, keeping existing plants");
            return;
        }
        
        // Check if we have saved data for the talking plants
        bool hasTalkingPlantData = false;
        foreach (var plantData in CurrentGameState.plants)
        {
            if (plantData.plantType.Contains("Talking"))
            {
                hasTalkingPlantData = true;
                break;
            }
        }
        
        // Destroy all regular non-talking plants 
        foreach (var plant in existingPlants.Values)
        {
            Destroy(plant);
        }
        
        // Create a map of plant data by position ID
        Dictionary<string, SerializedPlant> plantDataByPosition = new Dictionary<string, SerializedPlant>();
        foreach (var plantData in CurrentGameState.plants)
        {
            // Skip plants marked as harvested 
            if (plantData.isHarvested)
                continue;
                
            string posId = plantData.positionId;
            plantDataByPosition[posId] = plantData;
        }
        
        // Special handling for talking plants: Instead of destroying them,
        // update their properties
        if (existingTalkingPlants.Count > 0)
        {
            foreach (var entry in existingTalkingPlants)
            {
                string plantName = entry.Key;
                GameObject plantObj = entry.Value;
                Vector3 pos = plantObj.transform.position;
                string posId = $"{Mathf.RoundToInt(pos.x * 10)}-{Mathf.RoundToInt(pos.y * 10)}";
                
                // Check if we have saved data for this position
                if (plantDataByPosition.TryGetValue(posId, out SerializedPlant matchingData))
                {
                    // Found saved data for this position - update the existing plant
                    TalkingPlant talkingPlant = talkingPlantComponents[plantName];
                    
                    if (talkingPlant != null)
                    {
                        // Ensure we have a valid growth stage
                        if (matchingData.growthStage < 0)
                            matchingData.growthStage = 0;
                            
                        if (talkingPlant.growthStages != null && talkingPlant.growthStages.Length > 0)
                        {
                            if (matchingData.growthStage >= talkingPlant.growthStages.Length)
                                matchingData.growthStage = talkingPlant.growthStages.Length - 1;
                            
                            talkingPlant.currentStage = matchingData.growthStage;
                            talkingPlant.isWatered = matchingData.isWatered;
                            
                            // Force update the visuals right away
                            talkingPlant.UpdateVisuals();
                        }
                        
                        // Set harvestable state if applicable
                        HarvestablePlant harvestable = plantObj.GetComponent<HarvestablePlant>();
                        if (harvestable != null)
                        {
                            harvestable.isFullyGrown = matchingData.isFullyGrown;
                        }
                        
                        Debug.Log($"Updated existing talking plant: {plantName} at position {pos}, stage {matchingData.growthStage}");
                        
                        // Remove from the list of plants to restore
                        plantDataByPosition.Remove(posId);
                    }
                }
                else
                {
                    // No saved data for this plant
                    Debug.Log($"Keeping existing talking plant with no saved data: {plantName}");
                }
            }
        }
        
        // Check if all talking plants are already handled
        bool allTalkingPlantsHandled = true;
        foreach (var entry in plantDataByPosition)
        {
            if (entry.Value.plantType.Contains("Talking"))
            {
                allTalkingPlantsHandled = false;
                break;
            }
        }
        
        // Only destroy unmatched talking plants if need to recreate them
        if (hasTalkingPlantData && !allTalkingPlantsHandled)
        {
            foreach (var plantObj in existingTalkingPlants.Values)
            {
                Vector3 pos = plantObj.transform.position;
                string posId = $"{Mathf.RoundToInt(pos.x * 10)}-{Mathf.RoundToInt(pos.y * 10)}";
                
                // Only destroy if didn't already update it
                if (plantDataByPosition.ContainsKey(posId))
                {
                    Debug.Log($"Destroying talking plant to recreate: {plantObj.name}");
                    Destroy(plantObj);
                }
            }
        }
        
        // Restore plants from saved state
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("Cannot load plant state: InventoryManager not found!");
            return;
        }
        
        // Build a cache of all prefabs for faster lookup
        Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        foreach (var kvp in inventoryManager.seedToPlantPrefabs)
        {
            if (kvp.Value != null)
            {
                string prefabName = kvp.Value.name;
                prefabCache[prefabName] = kvp.Value;
                
                // add simplified versions of the name for matching
                if (prefabName.Contains("Plant"))
                {
                    string simpleName = prefabName.Replace("Plant", "").Trim();
                    if (!prefabCache.ContainsKey(simpleName))
                        prefabCache[simpleName] = kvp.Value;
                }
            }
        }
        
        Debug.Log($"Built prefab cache with {prefabCache.Count} entries");
        
        // Process remaining plants that need to be created
        int restoredCount = 0;
        foreach (var plantData in plantDataByPosition.Values)
        {
            // Try to find the prefab for this plant
            GameObject plantPrefab = FindPlantPrefab(plantData.plantType, prefabCache);
            
            if (plantPrefab == null)
            {
                Debug.LogWarning($"Could not find prefab for plant: {plantData.plantType}, skipping");
                continue;
            }
            
            // Instantiate the plant
            GameObject plantInstance = Instantiate(plantPrefab, plantData.position, Quaternion.identity);
            restoredCount++;
            
            // Set plant properties
            TalkingPlant talkingPlant = plantInstance.GetComponent<TalkingPlant>();
            if (talkingPlant != null)
            {
                // Ensure we have a valid growth stage
                if (plantData.growthStage < 0)
                    plantData.growthStage = 0;
                
                if (talkingPlant.growthStages != null && talkingPlant.growthStages.Length > 0)
                {
                    if (plantData.growthStage >= talkingPlant.growthStages.Length)
                        plantData.growthStage = talkingPlant.growthStages.Length - 1;
                    
                    talkingPlant.currentStage = plantData.growthStage;
                    talkingPlant.isWatered = plantData.isWatered;
                    
                    // Force update the visuals right away
                    talkingPlant.UpdateVisuals();
                }
                else
                {
                    Debug.LogError($"Plant {plantData.plantType} has no growth stages defined!");
                }
                
                Debug.Log($"Restored plant: {plantData.plantType} at position {plantData.position} ({plantData.positionId}), stage {plantData.growthStage}");
            }
            
            // Set harvestable state
            HarvestablePlant harvestable = plantInstance.GetComponent<HarvestablePlant>();
            if (harvestable != null)
            {
                harvestable.isFullyGrown = plantData.isFullyGrown;
                // We don't need to process isHarvested as harvested plants are now destroyed
            }
        }
        
        Debug.Log($"Loaded {restoredCount} plants of {CurrentGameState.plants.Count} total");
    }
    
    // Helper method to find the correct prefab for a plant
    private GameObject FindPlantPrefab(string plantType, Dictionary<string, GameObject> prefabCache)
    {
        // Exact match
        if (prefabCache.TryGetValue(plantType, out GameObject prefab))
            return prefab;
            
        // Special handling for Talking plants
        if (plantType.Contains("Talking"))
        {
            // Extract the core plant name like Grape from Talking Grape Plant
            string[] parts = plantType.Split(' ');
            if (parts.Length >= 3)
            {
                string corePlantName = parts[1]; // "Grape", "Pumpkin", etc.
                
                // Look for any prefab that contains this core name
                foreach (var kvp in prefabCache)
                {
                    if (kvp.Key.Contains(corePlantName))
                    {
                        Debug.Log($"Found match for talking plant: {plantType} -> {kvp.Key}");
                        return kvp.Value;
                    }
                }
                
                // looking directly in the scene for a prefab with this name
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains(corePlantName) && obj.GetComponent<TalkingPlant>() != null)
                    {
                        Debug.Log($"Found matching talking plant in scene: {obj.name}");
                        return obj;
                    }
                }
            }
        }
            
        // Try different variations of the name
        foreach (var key in prefabCache.Keys)
        {
            // Check if the key contains the plant type or vice versa
            if (key.Contains(plantType) || plantType.Contains(key))
            {
                Debug.Log($"Found partial match: {plantType} -> {key}");
                return prefabCache[key];
            }
                
            // Remove common suffixes/prefixes and check again
            string simplifiedKey = key.Replace("Plant", "").Replace("Prefab", "").Replace("Talking", "").Trim();
            string simplifiedType = plantType.Replace("Plant", "").Replace("Prefab", "").Replace("Talking", "").Trim();
            
            if (simplifiedKey == simplifiedType || 
                simplifiedKey.Contains(simplifiedType) || 
                simplifiedType.Contains(simplifiedKey))
            {
                Debug.Log($"Found simplified match: {plantType} ({simplifiedType}) -> {key} ({simplifiedKey})");
                return prefabCache[key];
            }
        }
        
        // Special case: check if this is a special pre-placed plant
        // If we get here and it has "Talking" in the name, we'll try to see if
        // there's a similarly named prefab already in the scene
        if (plantType.Contains("Talking"))
        {
            Debug.Log($"Trying last resort for talking plant: {plantType}");
            
            // Look for similar plants in the scene
            TalkingPlant[] existingPlants = FindObjectsOfType<TalkingPlant>();
            foreach (var p in existingPlants)
            {
                if (p.gameObject.name.Contains(plantType) || plantType.Contains(p.gameObject.name))
                {
                    Debug.Log($"Found similar talking plant in scene: {p.gameObject.name}");
                    return p.gameObject;
                }
            }
        }
        
        return null;
    }

    // Used to register all seeds in the scene during Start
    public void RegisterSeedInScene(string seedName, Vector3 position)
    {
        Vector3 roundedPos = position;
        string posId = $"{Mathf.RoundToInt(roundedPos.x * 10)}-{Mathf.RoundToInt(roundedPos.y * 10)}";
        
        // Check if we already have this seed in our list
        bool found = false;
        foreach (var seed in CurrentGameState.seedPickups)
        {
            if (seed.positionId == posId)
            {
                // We already know about this seed, just ensure the name is correct
                if (seed.seedName != seedName)
                {
                    seed.seedName = seedName;
                    Debug.Log($"Updated seed name at {posId}: {seedName}");
                }
                found = true;
                break;
            }
        }
        
        // If don't have it yet, add it
        if (!found)
        {
            SerializedSeedPickup newPickup = new SerializedSeedPickup
            {
                seedName = seedName,
                position = roundedPos,
                isPickedUp = false 
            };
            
            CurrentGameState.seedPickups.Add(newPickup);
            Debug.Log($"Registered new seed in scene: {seedName} at {posId}");
        }
    }
    
    // Call this whenever a seed is picked up
    public void RecordSeedPickup(string seedName, Vector3 position)
    {
        Vector3 roundedPos = position;
        string posId = $"{Mathf.RoundToInt(roundedPos.x * 10)}-{Mathf.RoundToInt(roundedPos.y * 10)}";
        
        Debug.Log($"Recording seed pickup: {seedName} at position {posId}");
        
        // Add this position ID to our master list of picked up seeds
        allPickedUpSeedPositionIds.Add(posId);
        Debug.Log($"Added to master list of picked up seeds, total: {allPickedUpSeedPositionIds.Count}");
        
        // Check if we already have this seed in our list
        bool found = false;
        foreach (var seed in CurrentGameState.seedPickups)
        {
            if (seed.positionId == posId)
            {
                // Update it to be picked up
                seed.isPickedUp = true;
                found = true;
                Debug.Log($"Marked seed as picked up: {seedName} at {posId}");
                break;
            }
        }
        
        // If we don't have it yet, add it
        if (!found)
        {
            SerializedSeedPickup newPickup = new SerializedSeedPickup
            {
                seedName = seedName,
                position = roundedPos,
                isPickedUp = true
            };
            
            CurrentGameState.seedPickups.Add(newPickup);
            Debug.Log($"Added new picked up seed: {seedName} at {posId}");
        }
        
        // Save the game state immediately to persist this change
        SaveGameState();
    }
    
    private void SaveSeedPickupState()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"SaveSeedPickupState in scene: {currentScene}, master list has {allPickedUpSeedPositionIds.Count} picked up seeds");
        
        // Find all active seed pickups in the current scene
        SeedPickUp[] seedPickups = FindObjectsOfType<SeedPickUp>();
        Dictionary<string, SeedPickUp> currentScenePickups = new Dictionary<string, SeedPickUp>();
        
        if (seedPickups != null)
        {
            foreach (var seedPickup in seedPickups)
            {
                if (seedPickup != null && seedPickup.gameObject != null)
                {
                    Vector3 pos = seedPickup.transform.position;
                    string posId = $"{Mathf.RoundToInt(pos.x * 10)}-{Mathf.RoundToInt(pos.y * 10)}";
                    currentScenePickups[posId] = seedPickup;
                    
                    Debug.Log($"Found seed in scene: {seedPickup.seedName} at position {posId}");
                }
            }
        }
        
        // Create a new list that maintains all pickedup seeds from all scenes
        List<SerializedSeedPickup> updatedList = new List<SerializedSeedPickup>();
        
        // First, add all current non-picked up seeds from the current scene
        foreach (var kvp in currentScenePickups)
        {
            string posId = kvp.Key;
            SeedPickUp seedPickup = kvp.Value;
            
            // Skip if its in our master list of picked up seeds
            if (allPickedUpSeedPositionIds.Contains(posId))
            {
                Debug.Log($"Skipping seed in scene that is in master pickup list: {seedPickup.seedName} at {posId}");
                continue;
            }
            
            SerializedSeedPickup serializedPickup = new SerializedSeedPickup
            {
                seedName = seedPickup.seedName,
                position = seedPickup.transform.position,
                isPickedUp = false
            };
            
            updatedList.Add(serializedPickup);
            Debug.Log($"Added current scene seed to updated list: {seedPickup.seedName} at {posId}");
        }
        
        // Now go through existing list and add all picked up seeds from all scenes
        foreach (var pickup in CurrentGameState.seedPickups)
        {
            // Only keep picked up seeds
            if (pickup.isPickedUp || allPickedUpSeedPositionIds.Contains(pickup.positionId))
            {
                // Set isPickedUp to true to ensure consistency with master list
                pickup.isPickedUp = true;
                
                // Don't add duplicates
                bool isDuplicate = false;
                foreach (var existingPickup in updatedList)
                {
                    if (existingPickup.positionId == pickup.positionId)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                {
                    updatedList.Add(pickup);
                    Debug.Log($"Added picked up seed from previous state: {pickup.seedName} at {pickup.positionId}");
                }
            }
        }
        
        // Replace the list with our updated version
        CurrentGameState.seedPickups = updatedList;
        
        Debug.Log($"Saved {CurrentGameState.seedPickups.Count} seed pickups, master list has {allPickedUpSeedPositionIds.Count} picked up seeds");
    }
    
    private void LoadSeedPickupState()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"LoadSeedPickupState in scene: {currentScene}, master list has {allPickedUpSeedPositionIds.Count} picked up seeds");
        
        // Get all seed pickups in the scene
        Dictionary<string, SeedPickUp> existingSeedPickups = new Dictionary<string, SeedPickUp>();
        SeedPickUp[] seedPickups = FindObjectsOfType<SeedPickUp>();
        
        foreach (var seedPickup in seedPickups)
        {
            if (seedPickup != null && seedPickup.gameObject != null)
            {
                Vector3 pos = seedPickup.transform.position;
                string posId = $"{Mathf.RoundToInt(pos.x * 10)}-{Mathf.RoundToInt(pos.y * 10)}";
                
                existingSeedPickups[posId] = seedPickup;
                Debug.Log($"Found existing seed pickup: {seedPickup.seedName} at position ID: {posId}");
            }
        }
        
        // First, update our master list from the current state
        foreach (var pickup in CurrentGameState.seedPickups)
        {
            if (pickup.isPickedUp)
            {
                allPickedUpSeedPositionIds.Add(pickup.positionId);
            }
        }
        
        // Remove all seeds that have been picked up
        int removedCount = 0;
        foreach (var posId in allPickedUpSeedPositionIds)
        {
            if (existingSeedPickups.TryGetValue(posId, out SeedPickUp seedPickup))
            {
                Debug.Log($"Removing picked up seed: {seedPickup.seedName} at {posId}");
                Destroy(seedPickup.gameObject);
                removedCount++;
            }
        }
        
        Debug.Log($"Removed {removedCount} picked up seeds, master list has {allPickedUpSeedPositionIds.Count} entries");
    }

    // New methods to save/load harvested talking plants state
    private void SaveHarvestedTalkingPlantsState()
    {
        // Clear the current list
        CurrentGameState.harvestedTalkingPlantPositionIds.Clear();
        
        // Add all harvested talking plant positions
        foreach (string posId in harvestedTalkingPlantPositionIds)
        {
            CurrentGameState.harvestedTalkingPlantPositionIds.Add(posId);
        }
        
        Debug.Log($"Saved {CurrentGameState.harvestedTalkingPlantPositionIds.Count} harvested talking plant positions");
    }

    private void LoadHarvestedTalkingPlantsState()
    {
        // Clear the current set
        harvestedTalkingPlantPositionIds.Clear();
        
        // Add all the harvested talking plant positions from the saved state
        foreach (string posId in CurrentGameState.harvestedTalkingPlantPositionIds)
        {
            harvestedTalkingPlantPositionIds.Add(posId);
        }
        
        Debug.Log($"Loaded {harvestedTalkingPlantPositionIds.Count} harvested talking plant positions");
    }

    public void RecordTalkingPlantHarvested(Vector3 position)
    {
        string posId = $"{Mathf.RoundToInt(position.x * 10)}-{Mathf.RoundToInt(position.y * 10)}";
        harvestedTalkingPlantPositionIds.Add(posId);
        Debug.Log($"Recorded talking plant harvested at position {posId}. Total harvested: {harvestedTalkingPlantPositionIds.Count}");
        
        // Save the game state to persist this change
        SaveGameState();
    }
} 