using UnityEngine;

/// <summary>Laser tự hủy sau vài giây hoặc khi chạm thiên thạch.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class LaserProjectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float speed = 12f;
    [SerializeField, Min(0.1f)] private float lifetime = 3f;
    [SerializeField] private int asteroidBonus = 5;

    private void Awake()
    {
        Collider2D hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.up * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AsteroidMotion asteroid = other.GetComponent<AsteroidMotion>();
        if (asteroid == null)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(asteroidBonus);

        asteroid.DestroyByLaser();
        Destroy(gameObject);
    }
}
