using UnityEngine;

public class HeldSeedVisualFollow : MonoBehaviour
{
    public SpriteRenderer seedSprite;

    // match her hand position in each direction
    public Vector3 rightOffset = new Vector3(-0.1f, -0.2f, 0f);
    public Vector3 leftOffset  = new Vector3(-0.4f, -0.15f, 0f);
    public Vector3 downOffset  = new Vector3(0.1f, -0.25f, 0f);

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
            // Horizontal movement
            if (dir.x > 0)
            {
                // Walking right
                transform.localPosition = rightOffset;
                transform.localScale = originalScale;
                seedSprite.enabled = true;
            }
            else
            {
                // Walking left — keep facing forward, move to left hand
                transform.localPosition = leftOffset;
                transform.localScale = originalScale;
                seedSprite.enabled = true;
            }
        }
        else
        {
            if (dir.y > 0)
            {
                // Walking up — hide seed
                seedSprite.enabled = false;
            }
            else
            {
                // Walking down — show in front
                transform.localPosition = downOffset;
                transform.localScale = originalScale;
                seedSprite.enabled = true;
            }
        }
    }
}
