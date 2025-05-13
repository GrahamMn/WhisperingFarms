using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down;

    public static Vector2 lastMoveDir = Vector2.down;

    private Animator animator;
    public Animator hoeAnimator;

    [Header("Optional Movement Bounds")]
    public bool useBounds = false;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(inputX) > 0.01f)
        {
            movement = new Vector2(inputX, 0);
        }
        else if (Mathf.Abs(inputY) > 0.01f)
        {
            movement = new Vector2(0, inputY);
        }
        else
        {
            movement = Vector2.zero;
        }

        bool isMoving = movement.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            lastDirection = movement;
            lastMoveDir = movement.normalized;

            animator.SetFloat("LastMoveX", lastDirection.x);
            animator.SetFloat("LastMoveY", lastDirection.y);
        }

        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
        animator.SetBool("IsMoving", isMoving);

        if (hoeAnimator != null)
        {
            int direction = 0;
            if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
                direction = lastDirection.x > 0 ? 1 : 3;
            else if (Mathf.Abs(lastDirection.y) > 0)
                direction = lastDirection.y > 0 ? 0 : 2;

            hoeAnimator.SetInteger("Direction", direction);
        }
    }

    private void FixedUpdate()
    {
        if (movement != Vector2.zero)
        {
            Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

            if (useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, minPosition.x, maxPosition.x);
                newPosition.y = Mathf.Clamp(newPosition.y, minPosition.y, maxPosition.y);
            }

            RaycastHit2D[] hits = new RaycastHit2D[1];
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            filter.useTriggers = false;

            int hitCount = rb.Cast(movement.normalized, filter, hits, moveSpeed * Time.fixedDeltaTime);
            if (hitCount == 0)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                Debug.Log("Blocked by: " + hits[0].collider.name);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision with: " + collision.gameObject.name);
    }
    private void OnDrawGizmosSelected()
{
    if (!useBounds) return;

    Gizmos.color = Color.red;
    Vector3 center = new Vector3(
        (minPosition.x + maxPosition.x) / 2f,
        (minPosition.y + maxPosition.y) / 2f,
        0f
    );

    Vector3 size = new Vector3(
        Mathf.Abs(maxPosition.x - minPosition.x),
        Mathf.Abs(maxPosition.y - minPosition.y),
        0f
    );

    Gizmos.DrawWireCube(center, size);
}

}
