using UnityEngine;

public class WateringCanVisualFollow : MonoBehaviour
{
    public SpriteRenderer wateringCanSprite;
    public Vector3 sideOffset = new Vector3(0.25f, 0.5f, 0);

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        Vector2 dir = PlayerMovement.lastMoveDir.normalized;

        // Flip direction based on X axis
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // Horizontal
            if (dir.x > 0)
            {
                transform.localPosition = new Vector3(sideOffset.x, sideOffset.y, 0);
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                wateringCanSprite.enabled = true;
            }
            else
            {
                transform.localPosition = new Vector3(-sideOffset.x, sideOffset.y, 0);
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                wateringCanSprite.enabled = true;
            }
        }
        else
        {
            // Vertical
            if (dir.y > 0)
            {
                // Facing Up hide
                wateringCanSprite.enabled = false;
            }
            else
            {
                // Facing Down show, default side
                transform.localPosition = new Vector3(sideOffset.x, sideOffset.y, 0);
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                wateringCanSprite.enabled = true;
            }
        }
    }
}
