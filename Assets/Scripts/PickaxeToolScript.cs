using UnityEngine;
using UnityEngine.Tilemaps;

public class PickaxeToolScript : MonoBehaviour
{
    private Animator animator;
    private Transform playerTransform;
    private Tilemap soilTilemap;

    public string toolName = "Pickaxe Tool";
    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerTransform = GameObject.FindWithTag("Player").transform;

        // Auto-assign Soil Tilemap
        GameObject soilTilemapObj = GameObject.Find("Soil Tilemap");
        if (soilTilemapObj != null)
        {
            soilTilemap = soilTilemapObj.GetComponent<Tilemap>();
        }
        else
        {
            Debug.LogError("PickaxeToolScript: Soil Tilemap not found in scene.");
        }
    }

    void Update()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.GetSelectedTool() != toolName)
            return;

        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastDirection = moveInput.normalized;
        }

        // Animate facing direction
        int dirInt = DirectionToInt(lastDirection);
        if (animator != null)
            animator.SetInteger("Direction", dirInt);

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryBreakTile();
        }
    }
    private void TryBreakTile()
    {
        if (soilTilemap == null)
        {
            Debug.LogWarning("Soil Tilemap is not assigned.");
            return;
        }

        Vector3Int targetGridPos = soilTilemap.WorldToCell(
            playerTransform.position + new Vector3(lastDirection.x, lastDirection.y, 0)
        );

        if (!soilTilemap.HasTile(targetGridPos))
        {
            Debug.Log(" No tile to destroy at " + targetGridPos);
            return;
        }

        // Match SeedPlanter logic exactly
        Vector3 center = soilTilemap.CellToWorld(targetGridPos) + new Vector3(0.5f, 0.5f, 0f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<TalkingPlant>() != null)
            {
                Debug.Log("Pickaxe blocked — plant exists at " + targetGridPos);
                return;
            }
        }

        soilTilemap.SetTile(targetGridPos, null);
        Debug.Log("Pickaxe destroyed soil tile at " + targetGridPos);
    }

    private int DirectionToInt(Vector2 direction)
    {
        if (direction.y > 0) return 0; // Up
        if (direction.x > 0) return 1; // Right
        if (direction.y < 0) return 2; // Down
        if (direction.x < 0) return 3; // Left
        return 0;
    }
}
