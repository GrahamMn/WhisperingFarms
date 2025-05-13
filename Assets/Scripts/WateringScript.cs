using UnityEngine;

public class WateringScript : MonoBehaviour
{
     public float wateringRange = 1.5f;

    void Update()
    {
        if (InventoryManager.Instance == null)
            return;

        if (InventoryManager.Instance.GetSelectedTool() != "Watering Can")
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] nearbyPlants = Physics2D.OverlapCircleAll(transform.position, wateringRange);
            Collider2D closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in nearbyPlants)
            {
                TalkingPlant plant = col.GetComponent<TalkingPlant>();
                if (plant != null)
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = col;
                    }
                }
            }

            if (closest != null)
            {
                closest.GetComponent<TalkingPlant>().ReceiveWater();
                Debug.Log("Watered plant: " + closest.name);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wateringRange);
    }
}

