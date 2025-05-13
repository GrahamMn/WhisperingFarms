using TMPro;
using UnityEngine;

public class SpeechBubbleController : MonoBehaviour
{
    public TextMeshProUGUI textField;

    public void ShowText(string text, float duration = 2f)
    {
        textField.text = text;
        gameObject.SetActive(true);
        CancelInvoke(); 
        Invoke(nameof(Hide), duration);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
