using UnityEngine;
using System.Collections;

public class ArcherBehavior : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float arrowSpeed = 10f;

    private Transform currentTarget;
    private bool canShoot = true;
    private Animator animator;

    private Coroutine shootingCoroutine;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (currentTarget != null && canShoot && shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootWithInterval());
        }
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;

        if (currentTarget == null && shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
            SetShootingAnimation(false);
        }
    }

    private IEnumerator ShootWithInterval()
    {
        while (currentTarget != null)
        {
            SetShootingAnimation(true);
            yield return new WaitForSeconds(shootInterval);
            if (currentTarget != null)
            {
                ShootArrowEvent();
            }
            SetShootingAnimation(false);
        }

        shootingCoroutine = null;
    }

    public void ShootArrowEvent()
    {
        if (currentTarget == null) return;

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        Vector2 direction = (currentTarget.position - shootPoint.position).normalized;
        arrow.transform.right = direction;
        rb.velocity = direction * arrowSpeed;
    }

    private void SetShootingAnimation(bool isShooting)
    {
        if (animator != null)
        {
            animator.SetBool("isShooting", isShooting);
        }
    }

    private void OnDisable()
    {
        SetShootingAnimation(false);
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
        }
        shootingCoroutine = null;
    }
}
