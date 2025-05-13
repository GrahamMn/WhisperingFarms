using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public GameObject shopPanel;
    public Transform buyButtonContainer;
    public Transform sellButtonContainer;
    public GameObject buttonPrefab;

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public GameObject plantPrefab;
        public int buyPrice;
        public int sellPrice;
    }

    public ShopItem[] shopItems;

    void Start()
    {
        shopPanel.SetActive(false);
        PopulateShop();
    }

    public void ToggleShop()
    {
        if (shopPanel.activeSelf)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            PopulateShop(); 
            shopPanel.SetActive(true);
        }
    }

    void PopulateShop()
    {
        // Clear buttons but keep headers
        foreach (Transform child in buyButtonContainer)
            if (child.name != "BuyHeader") Destroy(child.gameObject);
        foreach (Transform child in sellButtonContainer)
            if (child.name != "SellHeader") Destroy(child.gameObject);

        foreach (ShopItem item in shopItems)
        {
            string trimmedName = item.itemName.Trim();
            bool isSeed = trimmedName.ToLower().EndsWith("seed");

            // Create Buy button for seeds
            if (isSeed)
            {
                GameObject buyBtn = Instantiate(buttonPrefab, buyButtonContainer);
                buyBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Buy {trimmedName} - ${item.buyPrice}";
                buyBtn.GetComponent<Button>().onClick.AddListener(() => BuyItem(item));
            }

            // Create Sell button for harvested crops 
            if (!isSeed)
            {
                bool hasHarvest = InventoryManager.Instance.HasHarvest(trimmedName);
                Debug.Log($"Checking if player can sell '{trimmedName}': {hasHarvest}");

                if (hasHarvest)
                {
                    GameObject sellBtn = Instantiate(buttonPrefab, sellButtonContainer);
                    sellBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Sell {trimmedName} - ${item.sellPrice}";
                    sellBtn.GetComponent<Button>().onClick.AddListener(() => SellItem(item));
                }
            }
        }
    }

    void BuyItem(ShopItem item)
    {
        if (MoneyManager.Instance.TrySpend(item.buyPrice))
        {
            InventoryManager.Instance.AddItem(item.itemName, item.plantPrefab);
        }
        else
        {
            Debug.Log("Not enough money to buy " + item.itemName);
        }
    }

    void SellItem(ShopItem item)
{
    if (item == null) return;

    string itemName = item.itemName.Trim();

    if (itemName.ToLower().EndsWith("seed"))
    {
        Debug.Log("Selling seeds is not allowed.");
        return;
    }

    if (InventoryManager.Instance.TryRemoveHarvest(itemName))
    {
        MoneyManager.Instance.AddMoney(item.sellPrice);
        PopulateShop(); 
    }
    else
    {
        Debug.LogWarning($"Tried to sell {itemName} but none in inventory.");
    }
}

}
