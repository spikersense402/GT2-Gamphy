using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // Time before the arrow is destroyed

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the arrow after a certain time
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Arrow collided with: " + other.name + " with tag: " + other.tag); // Log collision details

        if (other.CompareTag("Goblin")) // Check if the arrow hits a goblin
        {
            GoblinCollector goblin = other.GetComponent<GoblinCollector>();
            if (goblin != null)
            {
                goblin.OnHitByArrow(); // Call the goblin's hit response
                Debug.Log("Goblin hit by arrow!"); // Log successful hit
            }

            Destroy(gameObject); // Destroy the arrow after hitting the goblin
        }
        else
        {
            Debug.Log("Arrow hit something else: " + other.tag); // Log if the arrow hits something else
        }
    }
}
