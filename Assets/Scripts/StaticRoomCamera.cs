using UnityEngine;

[RequireComponent(typeof(Camera))]
public class StaticRoomCamera : MonoBehaviour
{
    public BoxCollider2D roomBounds;

    public void CenterCamera()
    {
        if (roomBounds == null) return;
        Vector3 center = roomBounds.bounds.center;
        transform.position = new Vector3(center.x, center.y, transform.position.z);
    }

    void Start()
    {
        CenterCamera();

        Camera cam = GetComponent<Camera>();
        if (roomBounds != null)
        {
            Bounds bounds = roomBounds.bounds;

            float roomHeight = bounds.size.y;

            // fit height let FixedAspect handle width via black bars
            cam.orthographicSize = roomHeight / 2f;
        }
    }
}
