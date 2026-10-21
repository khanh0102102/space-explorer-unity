using UnityEngine;

/// <summary>Thiên thạch trôi ngẫu nhiên, tự xoay và quay lại từ mép màn hình.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class AsteroidMotion : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float minSpeed = 0.65f;
    [SerializeField, Min(0.1f)] private float maxSpeed = 2.2f;
    [SerializeField] private float minRotation = -65f;
    [SerializeField] private float maxRotation = 65f;

    private Vector2 driftDirection;
    private float driftSpeed;
    private float rotationSpeed;
    private SpawnManager owner;
    private bool removed;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        driftDirection = Random.insideUnitCircle.normalized;
        if (driftDirection.sqrMagnitude < 0.01f)
            driftDirection = Vector2.down;
        driftSpeed = Random.Range(minSpeed, maxSpeed);
        rotationSpeed = Random.Range(minRotation, maxRotation);
    }

    public void SetOwner(SpawnManager spawnManager)
    {
        owner = spawnManager;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        transform.position += (Vector3)(driftDirection * driftSpeed * Time.deltaTime);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        WrapAtScreenEdges();
    }

    private void WrapAtScreenEdges()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        float halfHeight = cam.orthographicSize + 0.7f;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 p = transform.position;

        if (p.x < -halfWidth) p.x = halfWidth;
        else if (p.x > halfWidth) p.x = -halfWidth;
        if (p.y < -halfHeight) p.y = halfHeight;
        else if (p.y > halfHeight) p.y = -halfHeight;

        transform.position = p;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (removed || other.GetComponent<PlayerController>() == null)
            return;

        // Theo yêu cầu bài: va chạm thiên thạch trừ điểm và kết thúc lượt chơi.
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerCrashed();
    }

    public void DestroyByLaser()
    {
        if (removed)
            return;

        removed = true;
        if (owner != null)
            owner.NotifyAsteroidDestroyed(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!removed && owner != null)
            owner.NotifyAsteroidDestroyed(this);
    }
}
