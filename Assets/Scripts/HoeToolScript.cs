using UnityEngine;
using UnityEngine.Tilemaps;

public class HoeToolScript : MonoBehaviour
{
    private Animator animator;
    private Transform playerTransform;

    public SoilTiller soilTiller;
    public string toolName = "Hoe Tool";

    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerTransform = GameObject.FindWithTag("Player").transform;

        // Auto-assign SoilTiller and Tilemap references at runtime
        if (soilTiller == null)
        {
            soilTiller = GetComponent<SoilTiller>();
        }

        if (soilTiller != null)
        {
            if (soilTiller.GroundTilemap == null)
            {
                var groundTilemapGO = GameObject.Find("Ground Tilemap");
                if (groundTilemapGO != null)
                    soilTiller.GroundTilemap = groundTilemapGO.GetComponent<Tilemap>();
            }

            if (soilTiller.soilTilemap == null)
            {
                var soilTilemapGO = GameObject.Find("Soil Tilemap");
                if (soilTilemapGO != null)
                    soilTiller.soilTilemap = soilTilemapGO.GetComponent<Tilemap>();
            }

            if (soilTiller.soilTile == null)
            {
                soilTiller.soilTile = Resources.Load<TileBase>("BaseMap2@20_129");
            }
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

        // Animate direction
        int dirInt = DirectionToInt(lastDirection);
        if (animator != null)
            animator.SetInteger("Direction", dirInt);

        // Till on key press
        if (Input.GetKeyDown(KeyCode.E) && soilTiller != null)
        {
            Vector3 targetWorldPos = playerTransform.position + new Vector3(lastDirection.x, lastDirection.y, 0);
            Vector3Int targetGridPos = soilTiller.GroundTilemap.WorldToCell(targetWorldPos);

            soilTiller.TillSoil(targetGridPos);
        }
    }

    int DirectionToInt(Vector2 direction)
    {
        if (direction.y > 0) return 0; // Up
        if (direction.x > 0) return 1; // Right
        if (direction.y < 0) return 2; // Down
        if (direction.x < 0) return 3; // Left
        return 0;
    }
}
