using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    private bool isInRange = false;
    public ShopManager shopManager;

    void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            shopManager.ToggleShop();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;

            if (shopManager.shopPanel.activeSelf)
                shopManager.ToggleShop();
        }
    }
}
