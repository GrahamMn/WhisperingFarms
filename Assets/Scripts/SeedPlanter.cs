using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public struct SeedPrefabMapping
{
    public string seedName;
    public GameObject prefab;
}

public class SeedPlanter : MonoBehaviour
{
    public Tilemap soilTilemap;

    [Header("Seed to Plant Prefabs")]
    public List<SeedPrefabMapping> seedPrefabMappings = new List<SeedPrefabMapping>();

    public Dictionary<string, GameObject> seedToPlantPrefabs { get; private set; } = new Dictionary<string, GameObject>();

    void Start()
    {
        // Build the dictionary from the list
        foreach (var mapping in seedPrefabMappings)
        {
            if (!string.IsNullOrEmpty(mapping.seedName) && mapping.prefab != null)
            {
                seedToPlantPrefabs[mapping.seedName] = mapping.prefab;
            }
        }
        // If InventoryManager seedToPlantPrefabs is not equivalent to this one update it
        if (InventoryManager.Instance.seedToPlantPrefabs != seedToPlantPrefabs)
        {
            InventoryManager.Instance.seedToPlantPrefabs = seedToPlantPrefabs;
        }

        // Autoassign soil tilemap if missing
        if (soilTilemap == null)
        {
            GameObject soilObj = GameObject.Find("Soil Tilemap");
            if (soilObj != null)
                soilTilemap = soilObj.GetComponent<Tilemap>();
        }
    }

    public void Plant(string seedName)
    {
        Debug.Log("🪴 SeedPlanter.Plant called with: " + seedName);


        if (string.IsNullOrEmpty(seedName)) return;

        Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;
        Vector3Int gridPos = soilTilemap.WorldToCell(playerPos);
        TileBase tile = soilTilemap.GetTile(gridPos);

        if (tile == null)
        {
            Debug.Log("No tilled soil at: " + gridPos);
            return;
        }

        if (IsPlantAlreadyThere(gridPos))
        {
            Debug.Log("Plant already exists at: " + gridPos);
            return;
        }

        GameObject plantPrefab = seedToPlantPrefabs.GetValueOrDefault(seedName);
        if (plantPrefab == null)
        {
            Debug.LogError("No plant prefab mapped for seed: " + seedName);
            return;
        }

        // First spawn the plant
        Vector3 spawnPos = soilTilemap.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0f);
        Instantiate(plantPrefab, spawnPos, Quaternion.identity);
        
        // Store current quantity before removing
        bool lastSeed = InventoryManager.Instance.GetItemCount(seedName) <= 1;
        
        // Remove the item from inventory
        InventoryManager.Instance.RemoveOneItem(seedName);
        
        // Only deselect if that was the last seed
        if (lastSeed)
        {
            InventoryManager.Instance.DeselectSeed();
        }
        else
        {
            // If not the last seed, update the held visual
            InventoryManager.Instance.RefreshHeldItem();
        }

        Debug.Log($"Successfully planted {seedName} at {gridPos}");
    }

    private bool IsPlantAlreadyThere(Vector3Int gridPos)
    {
        Vector3 center = soilTilemap.CellToWorld(gridPos) + new Vector3(0.5f, 0.5f, 0f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<TalkingPlant>() != null)
                return true;
        }
        return false;
    }
}
