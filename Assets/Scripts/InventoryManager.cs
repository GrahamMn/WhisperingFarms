using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI inventoryText;
    public GameObject seedButtonPrefab;
    public Transform buttonGroup;

    [Header("Tool GameObjects (Auto-assigned)")]
    public GameObject wateringCanObject;
    public GameObject hoeToolObject;
    public GameObject pickaxeToolObject;

    [Header("Held Item Visual")]
    public Transform heldItemAnchor;

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    private Dictionary<string, int> harvestCounts = new Dictionary<string, int>();
    private Dictionary<string, GameObject> buttonInstances = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> tools = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> seedToPlantPrefabs = new Dictionary<string, GameObject>();

    private string selectedSeed = "";
    private string selectedTool = "";
    private bool isInventoryOpen = false;

    private GameObject currentHeldItem;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Watering Can Tool
            // _________________

            if (wateringCanObject == null)
                wateringCanObject = player.transform.Find("WateringCan")?.gameObject;

            if (wateringCanObject != null && !tools.ContainsKey("Watering Can"))
            {
                tools["Watering Can"] = wateringCanObject;
                wateringCanObject.SetActive(false);
                AddTool("Watering Can");
            }
            
            // Hoe Tool
            // ________

            if (hoeToolObject == null)
                hoeToolObject = player.transform.Find("HoeTool")?.gameObject;


            if (hoeToolObject != null && !tools.ContainsKey("Hoe Tool"))
            {
                tools["Hoe Tool"] = hoeToolObject;
                hoeToolObject.SetActive(false);
                AddTool("Hoe Tool");
            }

            // Pickaxe Tool
            // ____________

            if (pickaxeToolObject == null)
                pickaxeToolObject = player.transform.Find("PickaxeTool")?.gameObject;

            if (pickaxeToolObject != null && !tools.ContainsKey("Pickaxe Tool"))
            {
                tools["Pickaxe Tool"] = pickaxeToolObject;
                pickaxeToolObject.SetActive(false);
                AddTool("Pickaxe Tool");
            }
            else if (pickaxeToolObject == null)
            {
                Debug.LogWarning("PickaxeTool GameObject not found on player.");
            }

            // Only find heldItemAnchor if not already assigned
            if (heldItemAnchor == null)
                heldItemAnchor = player.transform.Find("HeldItemAnchor");

            if (heldItemAnchor == null)
                Debug.LogError("HeldItemAnchor was not found on the Player!");
            else
                Debug.Log("Found HeldItemAnchor: " + heldItemAnchor.name);


        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSeedByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSeedByIndex(1);
    }

    public void AddItem(string itemName, GameObject plantPrefab = null)
    {
        if (itemCounts.ContainsKey(itemName))
        {
            itemCounts[itemName]++;
        }
        else
        {
            itemCounts[itemName] = 1;
            CreateInventoryButton(itemName);
        }

        if (plantPrefab != null && !seedToPlantPrefabs.ContainsKey(itemName))
        {
            seedToPlantPrefabs[itemName] = plantPrefab;
        }

        UpdateInventoryUI();
    }

    public void RemoveOneItem(string itemName)
    {
        if (itemCounts.ContainsKey(itemName))
        {
            itemCounts[itemName]--;

            if (itemCounts[itemName] <= 0)
            {
                itemCounts.Remove(itemName);
                //  let the caller handle it
                // if (selectedSeed == itemName) selectedSeed = "";

                if (buttonInstances.TryGetValue(itemName, out GameObject button))
                {
                    Destroy(button);
                    buttonInstances.Remove(itemName);
                }
            }

            UpdateInventoryUI();
        }
    }

    void CreateInventoryButton(string itemName)
    {
        if (buttonGroup == null)
        {
            Debug.LogError($"Cannot create button for {itemName}: buttonGroup is null");
            return;
        }

        if (seedButtonPrefab == null)
        {
            Debug.LogError($"Cannot create button for {itemName}: seedButtonPrefab is null");
            return;
        }

        if (buttonInstances.ContainsKey(itemName))
        {
            Debug.LogWarning($"Button for {itemName} already exists!");
            return;
        }

        Debug.Log($"Creating inventory button for {itemName}");

        GameObject newButton = Instantiate(seedButtonPrefab, buttonGroup);
        newButton.name = itemName + " Button";

        TextMeshProUGUI label = newButton.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = itemName;
        }
        else
        {
            Debug.LogWarning($"Label component not found on button for {itemName}");
        }

        Image icon = newButton.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null)
        {
            string path = "icon_" + itemName.Replace(" ", "");
            Sprite loadedIcon = Resources.Load<Sprite>(path);
            if (loadedIcon != null)
            {
                icon.sprite = loadedIcon;
            }
            else
            {
                Debug.LogWarning($"Icon not found for {itemName} at path: {path}");
            }
        }
        else
        {
            Debug.LogWarning($"Icon component not found on button for {itemName}");
        }

        Button btn = newButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners(); // Prevent double-triggering
            if (tools.ContainsKey(itemName))
            {
                btn.onClick.AddListener(() => SelectTool(itemName));
                Debug.Log($"Added tool selection listener for {itemName}");
            }
            else
            {
                btn.onClick.AddListener(() => SelectSeed(itemName));
                Debug.Log($"Added seed selection listener for {itemName}");
            }
        }
        else
        {
            Debug.LogWarning($"Button component not found on button for {itemName}");
        }

        buttonInstances[itemName] = newButton;
        Debug.Log($"Button for {itemName} created successfully");
    }

    public void SelectSeed(string seedName)
    {
        if (selectedSeed == seedName)
        {
            selectedSeed = "";
            if (currentHeldItem != null) Destroy(currentHeldItem);
            currentHeldItem = null;
            Debug.Log("Deselected seed: " + seedName);
            return;
        }

        selectedSeed = seedName;
        selectedTool = "";
        foreach (var tool in tools.Values)
            tool.SetActive(false);

        if (currentHeldItem != null)
            Destroy(currentHeldItem);

        string iconPath = "icon_" + seedName.Replace(" ", "");
        Sprite seedSprite = Resources.Load<Sprite>(iconPath);
        if (seedSprite == null)
        {
            Debug.LogWarning("Icon not found at: " + iconPath);
            return;
        }

        if (heldItemAnchor == null)
        {
            Debug.LogError(" heldItemAnchor is null!");
            return;
        }

        GameObject iconObj = new GameObject("HeldSeedIcon");
        iconObj.transform.SetParent(heldItemAnchor, false);

        iconObj.transform.localPosition = Vector3.zero;  // test from dead center
        iconObj.transform.localRotation = Quaternion.identity;
        iconObj.transform.localScale = Vector3.one * 0.7f;


        SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
        HeldSeedVisualFollow seedFollow = iconObj.AddComponent<HeldSeedVisualFollow>();
        seedFollow.seedSprite = sr;

        sr.sprite = seedSprite;
        sr.sortingLayerName = "Default"; // match character's sorting layer
        sr.sortingOrder = 10;            // slightly above player body
        sr.material = new Material(Shader.Find("Sprites/Default"));

        currentHeldItem = iconObj;

        Debug.Log("Selected seed: " + seedName);
    }

    private void SelectSeedByIndex(int index)
    {
        if (index >= 0 && index < itemCounts.Count)
        {
            int i = 0;
            foreach (var item in itemCounts)
            {
                if (i == index)
                {
                    SelectSeed(item.Key);
                    break;
                }
                i++;
            }
        }
    }

    public string GetSelectedSeed() => selectedSeed;

    public void AddTool(string toolName)
    {
        if (buttonInstances.ContainsKey(toolName))
        {
            Debug.Log($"Tool {toolName} already has a button, skipping creation");
            return;
        }

        Debug.Log($"Creating button for tool: {toolName}");
        CreateInventoryButton(toolName);
    }

    public void SelectTool(string toolName)
    {
        if (selectedTool == toolName)
        {
            selectedTool = "";
            foreach (var tool in tools.Values)
                tool.SetActive(false);
            Debug.Log("Unequipped tool: " + toolName);
            return;
        }

        selectedTool = toolName;
        selectedSeed = "";

        foreach (var tool in tools.Values)
            tool.SetActive(false);

        if (tools.ContainsKey(toolName))
        {
            tools[toolName].SetActive(true);
            Debug.Log("Equipped tool: " + toolName);
        }

        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }
    }

    public string GetSelectedTool() => selectedTool;

    public void AddHarvest(string itemName)
    {
        if (harvestCounts.ContainsKey(itemName))
            harvestCounts[itemName]++;
        else
        {
            harvestCounts[itemName] = 1;
            CreateInventoryButton(itemName);
        }

        Debug.Log($"Added harvest: {itemName} ({harvestCounts[itemName]})");
        UpdateInventoryUI();
    }

    public bool TryRemoveHarvest(string itemName)
    {
        if (harvestCounts.ContainsKey(itemName) && harvestCounts[itemName] > 0)
        {
            harvestCounts[itemName]--;

            if (harvestCounts[itemName] <= 0)
            {
                harvestCounts.Remove(itemName);

                if (buttonInstances.TryGetValue(itemName, out GameObject button))
                {
                    Destroy(button);
                    buttonInstances.Remove(itemName);
                }
            }

            UpdateInventoryUI();
            return true;
        }

        Debug.LogWarning($"Tried to sell {itemName} but none were in inventory.");
        return false;
    }

    public bool HasHarvest(string itemName)
    {
        return harvestCounts.ContainsKey(itemName) && harvestCounts[itemName] > 0;
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);
    }

    void UpdateInventoryUI()
    {
        foreach (var kvp in itemCounts)
        {
            if (buttonInstances.TryGetValue(kvp.Key, out GameObject btnObj))
            {
                TextMeshProUGUI label = btnObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                if (label != null)
                    label.text = kvp.Key + "\n(" + kvp.Value + ")";
            }
        }

        foreach (var kvp in harvestCounts)
        {
            if (!buttonInstances.ContainsKey(kvp.Key))
                CreateInventoryButton(kvp.Key);

            if (buttonInstances.TryGetValue(kvp.Key, out GameObject btnObj))
            {
                TextMeshProUGUI label = btnObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                if (label != null)
                    label.text = kvp.Key + "\n(" + kvp.Value + ")";
            }
        }

        if (inventoryText != null)
        {
            inventoryText.text = "Inventory:\n";
            foreach (var kvp in itemCounts)
                inventoryText.text += "- " + kvp.Key + " x" + kvp.Value + "\n";

            inventoryText.text += "\nHarvested:\n";
            foreach (var kvp in harvestCounts)
                inventoryText.text += "- " + kvp.Key + " x" + kvp.Value + "\n";
        }
    }

    public bool TryRemoveItem(string itemName)
    {
        if (itemCounts.ContainsKey(itemName) && itemCounts[itemName] > 0)
        {
            RemoveOneItem(itemName);
            return true;
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        if (itemCounts.ContainsKey(itemName))
            return itemCounts[itemName];
        return 0;
    }

    public bool HasItem(string itemName)
    {
        return itemCounts.ContainsKey(itemName) && itemCounts[itemName] > 0;
    }

    public void RefreshHeldItem()
    {
        // If still have a selected seed, refresh its visual
        if (!string.IsNullOrEmpty(selectedSeed))
        {
            // If there's a visual already destroy it to avoid duplicates
            if (currentHeldItem != null)
            {
                Destroy(currentHeldItem);
                currentHeldItem = null;
            }

            // Create a fresh visual with updated information
            string iconPath = "icon_" + selectedSeed.Replace(" ", "");
            Sprite seedSprite = Resources.Load<Sprite>(iconPath);
            if (seedSprite != null && heldItemAnchor != null)
            {
                GameObject iconObj = new GameObject("HeldSeedIcon");
                iconObj.transform.SetParent(heldItemAnchor, false);
                iconObj.transform.localPosition = Vector3.zero;
                iconObj.transform.localRotation = Quaternion.identity;
                iconObj.transform.localScale = Vector3.one * 0.7f;

                SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
                HeldSeedVisualFollow seedFollow = iconObj.AddComponent<HeldSeedVisualFollow>();
                seedFollow.seedSprite = sr;

                sr.sprite = seedSprite;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 10;
                sr.material = new Material(Shader.Find("Sprites/Default"));

                currentHeldItem = iconObj;
            }
        }
    }

    public void DeselectSeed()
    {
        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }
        selectedSeed = "";
    }

    // Added for GameManager persistence
    private void ClearInventory()
    {
        // Clear data structures
        itemCounts.Clear();
        harvestCounts.Clear();

        // Destroy UI buttons
        foreach (var button in buttonInstances.Values)
        {
            Destroy(button);
        }
        buttonInstances.Clear();

        // Clear selection
        selectedSeed = "";
        selectedTool = "";

        // Clear held item
        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }

        // Hide tools
        foreach (var tool in tools.Values)
        {
            tool.SetActive(false);
        }

        UpdateInventoryUI();
    }

    // Method for GameManager to restore harvests safely
    internal void RestoreHarvest(string itemName, int count)
    {
        Debug.Log($"Restoring harvest: {itemName} x{count}");

        // Store the count
        harvestCounts[itemName] = count;

        // Only create button if it doesn't exist
        if (!buttonInstances.ContainsKey(itemName))
        {
            Debug.Log($"Creating button for harvest: {itemName}");
            CreateInventoryButton(itemName);
        }
        else
        {
            Debug.Log($"Button for harvest {itemName} already exists");
        }

        // Update UI labels
        UpdateInventoryUI();
    }

    // Method for GameManager to restore tools safely
    internal void RestoreTools()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Cannot restore tools: Player not found!");
            return;
        }

        Debug.Log("Restoring tools...");

        // Always reset the tool references to be safe
        wateringCanObject = player.transform.Find("WateringCan")?.gameObject;
        hoeToolObject = player.transform.Find("HoeTool")?.gameObject;
        heldItemAnchor = player.transform.Find("HeldItemAnchor");

        if (wateringCanObject == null)
        {
            Debug.LogError("WateringCan not found on player!");
        }
        else
        {
            Debug.Log("WateringCan found on player!");
        }

        if (hoeToolObject == null)
        {
            Debug.LogError("HoeTool not found on player!");
        }
        else
        {
            Debug.Log("HoeTool found on player!");
        }

        // Clear the tools dictionary and readd the tools
        tools.Clear();
        buttonInstances.Remove("Watering Can");
        buttonInstances.Remove("Hoe Tool");
        buttonInstances.Remove("Pickaxe Tool");

        if (wateringCanObject != null)
        {
            tools["Watering Can"] = wateringCanObject;
            wateringCanObject.SetActive(false);
            AddTool("Watering Can");
            Debug.Log("Added Watering Can to inventory");
        }

        if (hoeToolObject != null)
        {
            tools["Hoe Tool"] = hoeToolObject;
            hoeToolObject.SetActive(false);
            AddTool("Hoe Tool");
            Debug.Log("Added Hoe Tool to inventory");
        }

        if (pickaxeToolObject != null)
        {
            tools["Pickaxe Tool"] = pickaxeToolObject;
            pickaxeToolObject.SetActive(false);
            AddTool("Pickaxe Tool");
            Debug.Log("Added Pickaxe Tool to inventory");
        }

        // Force an update of the UI
        UpdateInventoryUI();
    }

}
