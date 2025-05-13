using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    void Update()
    {
        if (MoneyManager.Instance != null)
        {
            moneyText.text = "Money: $" + MoneyManager.Instance.money;
        }
    }
}
