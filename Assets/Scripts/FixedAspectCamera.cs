using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    void Start()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1f)
        {
            // Add letterbox 
            Rect rect = cam.rect;

            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;

            cam.rect = rect;
        }
        else
        {
            // Add pillarbox 
            float scaleWidth = 1f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0f;

            cam.rect = rect;
        }
    }
}
