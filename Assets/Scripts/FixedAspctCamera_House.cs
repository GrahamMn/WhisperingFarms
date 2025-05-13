using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera_House : MonoBehaviour
{
    public float targetAspect = 16f / 9f;
    private int lastWidth, lastHeight;

    void Start()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        ApplyAspect();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplyAspect();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }

    void ApplyAspect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1f)
        {
            cam.rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            cam.rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
        }
    }
}
