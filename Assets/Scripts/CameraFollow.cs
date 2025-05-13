using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public string playerTag = "Player";
    public BoxCollider2D cameraBounds;

    private Transform target;
    private Camera cam;

    private float halfHeight;
    private float halfWidth;

    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        cam = GetComponent<Camera>();
        TryFindPlayer();

        if (cameraBounds != null)
        {
            minBounds = cameraBounds.bounds.min;
            maxBounds = cameraBounds.bounds.max;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            TryFindPlayer();
            return;
        }

        if (cameraBounds == null) return;

        // Forcing the camera to always render 30 units wide
        cam.orthographicSize = 50f / (2f * cam.aspect);
        UpdateCameraSize();

        Vector3 targetPos = target.position;

        float clampedX = Mathf.Clamp(
            targetPos.x,
            minBounds.x + halfWidth,
            maxBounds.x - halfWidth
        );

        float clampedY = Mathf.Clamp(
            targetPos.y,
            minBounds.y + halfHeight,
            maxBounds.y - halfHeight
        );


        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    void UpdateCameraSize()
    {
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (cameraBounds != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(cameraBounds.bounds.center, cameraBounds.bounds.size);
        }
    }
}
