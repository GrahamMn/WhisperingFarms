using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AutoCameraZoom : MonoBehaviour
{
    public float targetWorldHeight = 10f; // Match the vertical height of room

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.orthographicSize = targetWorldHeight / 2f;
    }
}
