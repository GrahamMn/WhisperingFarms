using UnityEngine;

public class Shopkeeper : MonoBehaviour
{
    public Animator animator;
    public ShopManager shopManager;

    private bool isPlayerNearby = false;

    private float lastGreetTime = -10f;
    public float greetCooldown = 2f; 

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (Time.time - lastGreetTime > greetCooldown)
            {
                animator.SetTrigger("Greet");
                lastGreetTime = Time.time;

                // Delay shop open to match greet animation
                Invoke(nameof(OpenShop), 1.0f); 
            }
            else
            {
               
                OpenShop();
            }
        }
    }

    void OpenShop()
    {
        animator.SetBool("Thinking", true);
        shopManager.ToggleShop();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Return to idle animation state
            animator.SetBool("Thinking", false);
        }
    }
}
