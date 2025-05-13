using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;
    public int money = 0; // Starting money

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool TrySpend(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"Spent ${amount}. Remaining: {money}");
            return true;
        }
        Debug.Log("Not enough money!");
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"Gained ${amount}. New total: {money}");
    }
}
