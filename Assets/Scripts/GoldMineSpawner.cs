using UnityEngine;
using DG.Tweening; 

public class GoldMineSpawner : MonoBehaviour
{
    [SerializeField] protected GameObject goldBagPrefab; 
    [SerializeField] protected float spawnInterval = 5f; 
    [SerializeField] protected Vector2 greenLandSize; 
    [SerializeField] protected Vector2 yellowLandSize; 
    [SerializeField] protected Vector2 greenLandCenter; 
    [SerializeField] protected Vector2 yellowLandCenter; 
    [SerializeField] protected float shootForce = 500f; 
    protected float timeSinceLastSpawn;

    void Update()
    {
        // Increment the timer
        timeSinceLastSpawn += Time.deltaTime;

        // If enough time has passed, spawn a gold bag
        if (timeSinceLastSpawn >= spawnInterval)
        {
            ShootGoldBag();
            timeSinceLastSpawn = 0f;
        }
    }

    protected virtual void ShootGoldBag()
    {
        // Randomly choose between the green land and yellow land
        bool shootToGreenLand = Random.value > 0.5f;

        // Set the size of the target area based on the chosen land
        Vector2 landSize = shootToGreenLand ? greenLandSize : yellowLandSize;
        Vector2 landCenter = shootToGreenLand ? greenLandCenter : yellowLandCenter;

        // Randomize the x and y positions within the chosen land area
        Vector2 randomPosition = new Vector2(
            Random.Range(-landSize.x / 2, landSize.x / 2),
            Random.Range(-landSize.y / 2, landSize.y / 2)
        );

        // Adjust the random position based on the land's center
        Vector2 targetPosition = landCenter + randomPosition;

        // Instantiate the gold bag prefab at the gold mine's position
        GameObject spawnedGoldBag = Instantiate(goldBagPrefab, transform.position, Quaternion.identity);

        // Add a Rigidbody2D to the gold bag if it doesn't already have one
        Rigidbody2D rb = spawnedGoldBag.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = spawnedGoldBag.AddComponent<Rigidbody2D>();
        }

        // Disable gravity so the gold bag flies in a straight line
        rb.gravityScale = 0;

        // Calculate the direction to shoot the gold bag
        Vector2 shootDirection = (targetPosition - (Vector2)transform.position).normalized;

        // Apply force to shoot the gold bag in the direction of the target
        rb.AddForce(shootDirection * shootForce);

        // Commented out the torque to remove rotation
        // rb.AddTorque(Random.Range(-50f, 50f));

        // Apply tweening animation (optional)
        AnimateGoldBag(spawnedGoldBag);
    }

    // Function to animate the gold bag with DOTween (optional)
    protected virtual void AnimateGoldBag(GameObject goldBag)
    {
        // Set the initial scale to 0 (so it starts "invisible")
        goldBag.transform.localScale = Vector3.zero;

        // Tween the scale from 0 to 1 over 0.5 seconds to create a pop-in effect
        goldBag.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
    }

    // This function will be called by Unity to draw gizmos in the Scene view
    private void OnDrawGizmos()
    {
        // Set the Gizmo color for the green land
        Gizmos.color = Color.green;

        // Draw the outline of the green land area (a rectangle)
        Gizmos.DrawWireCube(greenLandCenter, greenLandSize);

        // Set the Gizmo color for the yellow land
        Gizmos.color = Color.yellow;

        // Draw the outline of the yellow land area (a rectangle)
        Gizmos.DrawWireCube(yellowLandCenter, yellowLandSize);
    }
}
