using UnityEngine;

public class PickaxeVisualFollow : MonoBehaviour
{
    public SpriteRenderer pickaxeSprite;
    public Vector3 sideOffset = new Vector3(0.25f, 0.5f, 0);

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        Vector2 dir = PlayerMovement.lastMoveDir.normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // Left/Right
            if (dir.x > 0)
            {
                transform.localPosition = new Vector3(sideOffset.x, sideOffset.y, 0);
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                pickaxeSprite.enabled = true;
            }
            else
            {
                transform.localPosition = new Vector3(-sideOffset.x, sideOffset.y, 0);
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                pickaxeSprite.enabled = true;
            }
        }
        else
        {
            if (dir.y > 0)
            {
                // Facing Up
                pickaxeSprite.enabled = false;
            }
            else
            {
                // Facing Down
                transform.localPosition = new Vector3(sideOffset.x, sideOffset.y, 0);
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                pickaxeSprite.enabled = true;
            }
        }
    }
}
