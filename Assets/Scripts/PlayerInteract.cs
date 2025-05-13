using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 1.5f;
    public LayerMask plantLayer;
    public LayerMask soilLayer;

    void Update()
    {
        if (!Input.GetKeyDown(interactKey)) return;

        // Harvest or talk
        Collider2D[] plantHits = Physics2D.OverlapCircleAll(transform.position, interactRange, plantLayer);
        foreach (Collider2D hit in plantHits)
        {
            if (hit.TryGetComponent(out HarvestablePlant harvest) && harvest.isFullyGrown)
            {
                harvest.Harvest();
                return;
            }

            if (hit.TryGetComponent(out PlantDialogue talker))
            {
                talker.StartDialogue();
                return;
            }
        }

        // Watering
        if (InventoryManager.Instance != null && InventoryManager.Instance.GetSelectedTool() == "Watering Can")
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
            foreach (var col in hits)
            {
                if (col.TryGetComponent(out TalkingPlant tp))
                {
                    tp.ReceiveWater();
                    return;
                }
            }
        }

        // Tilling
        if (InventoryManager.Instance != null && InventoryManager.Instance.GetSelectedTool() == "Hoe Tool")
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, soilLayer);
            foreach (var col in hits)
            {
                if (col.TryGetComponent(out SoilTiller tiller))
                {
                    tiller.Till();
                    return;
                }
            }
        }

        // Planting
        if (InventoryManager.Instance != null)
        {
            string seed = InventoryManager.Instance.GetSelectedSeed();
            if (!string.IsNullOrEmpty(seed))
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, soilLayer);
                foreach (var col in hits)
                {
                    if (col.TryGetComponent(out SeedPlanter planter))
                    {
                        planter.Plant(seed);
                        return;
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
