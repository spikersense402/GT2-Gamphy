using UnityEngine;
using System.Linq;

public class GoblinCollector : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private Transform shedTransform;
    [SerializeField] private Vector2 homeCenter;
    [SerializeField] private float homeRadius = 5.0f;
    [SerializeField] private Vector2 greenZoneCenter;
    [SerializeField] private Vector2 greenZoneSize;
    [SerializeField] private GoldCountUI goldCountUI;
    [SerializeField] private float droppedBagCooldown = 1f;

    private GameObject currentGoldBag;
    private GameObject recentlyDroppedBag;
    private bool hasGoldBag = false;
    private Vector2 idlePosition;
    private bool isAtIdlePosition = false;
    private float droppedBagCooldownTimer = 0f;
    private bool isOnCooldown = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 previousPosition;
    private ArcherBehavior archer;

    private Vector2 currentVelocity;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        animator = GetComponent<Animator>();
        previousPosition = rb.position;
        idlePosition = GenerateRandomPointInCircle();
        archer = FindObjectOfType<ArcherBehavior>();

        if (archer == null)
        {
            Debug.LogError("Archer not found in the scene. Make sure an object with ArcherBehavior is in the scene.");
        }

        // Check for any null references in serialized fields
        if (shedTransform == null) Debug.LogError("Shed Transform is not assigned.");
        if (goldCountUI == null) Debug.LogError("Gold Count UI is not assigned.");
    }

    private void Update()
    {
        UpdateAnimator();
        CheckIfInGreenZone();
        HandleCooldown();
    }

    private void FixedUpdate()
    {
        if (!hasGoldBag && currentGoldBag == null)
        {
            FindNearestGoldBag();
        }

        if (!hasGoldBag)
        {
            if (currentGoldBag != null)
            {
                MoveToGoldBag();
            }
            else if (!isAtIdlePosition)
            {
                MoveToIdlePosition();
            }
        }
        else
        {
            MoveToShed();
        }

        CalculateVelocity();
    }

    private void FindNearestGoldBag()
    {
        var goldBags = GameObject.FindGameObjectsWithTag("GoldBag")
            .Where(gb => gb.activeSelf && (!isOnCooldown || gb != recentlyDroppedBag)).ToArray();

        var closestBag = goldBags
            .OrderBy(gb => (gb.transform.position - transform.position).sqrMagnitude)
            .FirstOrDefault();

        currentGoldBag = closestBag;

        if (currentGoldBag != null)
        {
            Debug.Log("Found nearest gold bag: " + currentGoldBag.name);
        }
        else
        {
            Debug.Log("No gold bags found.");
        }
    }

    private void MoveToGoldBag()
    {
        if (currentGoldBag != null)
        {
            isAtIdlePosition = false;
            MoveTowards(currentGoldBag.transform.position);
        }
    }

    private void MoveToShed()
    {
        if (shedTransform != null)
        {
            isAtIdlePosition = false;
            MoveTowards(shedTransform.position);
        }
    }

    private void MoveToIdlePosition()
    {
        if (Vector2.Distance(transform.position, idlePosition) < 0.1f)
        {
            isAtIdlePosition = true;
        }
        else
        {
            MoveTowards(idlePosition);
        }
    }

    private Vector2 GenerateRandomPointInCircle()
    {
        Vector2 randomPoint = Random.insideUnitCircle * homeRadius;
        return homeCenter + randomPoint;
    }

    private void MoveTowards(Vector2 target)
    {
        Vector2 direction = (target - rb.position).normalized;
        Vector2 movement = direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
        FlipSprite(target);
    }

    private void FlipSprite(Vector2 target)
    {
        spriteRenderer.flipX = target.x < transform.position.x;
    }

    private void CalculateVelocity()
    {
        currentVelocity = (rb.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = rb.position;
    }

    private void UpdateAnimator()
    {
        float velocityMagnitude = currentVelocity.sqrMagnitude;
        animator.SetBool("isWalking", velocityMagnitude > 0.01f);
    }

    private void CheckIfInGreenZone()
    {
        float halfWidth = greenZoneSize.x / 2;
        float halfHeight = greenZoneSize.y / 2;

        if (transform.position.x > greenZoneCenter.x - halfWidth &&
            transform.position.x < greenZoneCenter.x + halfWidth &&
            transform.position.y > greenZoneCenter.y - halfHeight &&
            transform.position.y < greenZoneCenter.y + halfHeight)
        {
            if (archer != null)
            {
                archer.SetTarget(transform);
                Debug.Log("Goblin is in the Green Zone! Archer should attack now.");
            }
        }
        else
        {
            if (archer != null)
            {
                archer.SetTarget(null);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Goblin collided with: " + other.name + " with tag: " + other.tag);

        if (other.CompareTag("Arrow")) // Check if collided object is an arrow
        {
            OnHitByArrow(); // Call the goblin's hit response directly
            Destroy(other.gameObject); // Destroy the arrow after hitting the goblin
            return; // Exit the method after handling the arrow collision
        }

        if (!hasGoldBag && other.CompareTag("GoldBag"))
        {
            if (isOnCooldown && other.gameObject == recentlyDroppedBag)
            {
                Debug.Log("Cannot pick up recently dropped bag. Still on cooldown.");
                return;
            }

            currentGoldBag = other.gameObject;
            currentGoldBag.SetActive(false);
            hasGoldBag = true;
            recentlyDroppedBag = null;
            Debug.Log("Picked up gold bag: " + currentGoldBag.name);
        }
        else if (hasGoldBag && other.transform == shedTransform)
        {
            Destroy(currentGoldBag);
            currentGoldBag = null;
            hasGoldBag = false;
            idlePosition = GenerateRandomPointInCircle();

            if (goldCountUI != null)
            {
                goldCountUI.UpdateGoldCount();
            }
        }
    }

    private void HandleCooldown()
    {
        if (isOnCooldown)
        {
            droppedBagCooldownTimer -= Time.deltaTime;
            if (droppedBagCooldownTimer <= 0)
            {
                isOnCooldown = false;
                recentlyDroppedBag = null;
                Debug.Log("Cooldown ended. Goblin can pick up the dropped bag again.");
            }
        }
    }

    public void OnHitByArrow()
    {
        Debug.Log("Goblin hit by an arrow!");

        if (hasGoldBag)
        {
            currentGoldBag.transform.position = transform.position;
            currentGoldBag.SetActive(true);
            recentlyDroppedBag = currentGoldBag;
            StartCooldown();

            hasGoldBag = false;
            currentGoldBag = null;

            FindNearestGoldBag();
        }
    }

    private void StartCooldown()
    {
        droppedBagCooldownTimer = droppedBagCooldown;
        isOnCooldown = true;
        Debug.Log($"Cooldown started for {droppedBagCooldown} seconds.");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(homeCenter, homeRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(greenZoneCenter, greenZoneSize);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(idlePosition, 0.1f);
        }
    }
}
