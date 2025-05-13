using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("Day Settings")]
    public float secondsPerDay = 180f;
    private float time;

    public enum TimeOfDay { Morning, Afternoon, Evening, Night }
    public TimeOfDay currentTimeOfDay;

    [Header("Lighting")]
    public Light2D globalLight;

    [Header("Clock UI")]
    public Image clockUIImage;
    public Sprite[] clockIcons; // 0 = Morning, 1 = Afternoon, 2 = Evening, 3 = Night
    public TextMeshProUGUI clockLabel;

    // Fade settings
    private string currentLabel = "";
    private float labelFadeSpeed = 5f;
    private float iconFadeSpeed = 5f;

    void Update()
    {
        time += Time.deltaTime;
        float dayProgress = (time % secondsPerDay) / secondsPerDay;

        // Target values
        Color targetColor = Color.white;
        Sprite targetIcon = null;
        string targetLabel = "";

        if (dayProgress < 0.25f)
        {
            currentTimeOfDay = TimeOfDay.Morning;
            targetColor = new Color(1f, 0.95f, 0.8f);
            targetIcon = clockIcons[0];
            targetLabel = "Morning";
        }
        else if (dayProgress < 0.5f)
        {
            currentTimeOfDay = TimeOfDay.Afternoon;
            targetColor = Color.white;
            targetIcon = clockIcons[1];
            targetLabel = "Afternoon";
        }
        else if (dayProgress < 0.75f)
        {
            currentTimeOfDay = TimeOfDay.Evening;
            targetColor = new Color(1f, 0.7f, 0.4f);
            targetIcon = clockIcons[2];
            targetLabel = "Evening";
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Night;
            targetColor = new Color(0.2f, 0.2f, 0.4f);
            targetIcon = clockIcons[3];
            targetLabel = "Night";
        }

        // Smooth light transition
        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * 1.5f);

        // Smooth icon fade in when changed
        if (clockUIImage.sprite != targetIcon)
        {
            clockUIImage.sprite = targetIcon;
            Color c = clockUIImage.color;
            clockUIImage.color = new Color(c.r, c.g, c.b, 0f); 
        }

        if (clockUIImage.color.a < 1f)
        {
            Color fadedIcon = clockUIImage.color;
            fadedIcon.a = Mathf.Lerp(fadedIcon.a, 1f, Time.deltaTime * iconFadeSpeed);
            clockUIImage.color = fadedIcon;
        }

        // Smooth label fade in when changed
        if (clockLabel.text != targetLabel)
        {
            clockLabel.text = targetLabel;
            clockLabel.color = new Color(clockLabel.color.r, clockLabel.color.g, clockLabel.color.b, 0f); 
            currentLabel = targetLabel;
        }

        if (clockLabel.color.a < 1f)
        {
            Color fadedLabel = clockLabel.color;
            fadedLabel.a = Mathf.Lerp(fadedLabel.a, 1f, Time.deltaTime * labelFadeSpeed);
            clockLabel.color = fadedLabel;
        }
    }
}
