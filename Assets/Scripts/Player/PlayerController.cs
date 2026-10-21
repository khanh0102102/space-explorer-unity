using UnityEngine;

/// <summary>
/// Điều khiển tàu bằng phím mũi tên hoặc WASD; nhấn Space để bắn laser.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 6f;

    [Header("Vũ khí")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0.05f)] private float fireCooldown = 0.24f;
    [SerializeField] private AudioClip laserSound;

    private Rigidbody2D body;
    private AudioSource audioSource;
    private float nextFireTime;
    private Camera mainCamera;
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Dự phòng để phím mũi tên/WASD vẫn hoạt động khi Input Manager đã được tùy chỉnh.
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) vertical = 1f;

        moveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextFireTime)
            Shoot();
    }

    private void FixedUpdate()
    {
        body.linearVelocity = moveInput * moveSpeed;
        KeepInsideCameraBounds();
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireCooldown;
        if (laserPrefab == null)
            return;

        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 0.55f;

        Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
        if (laserSound != null && audioSource != null)
            audioSource.PlayOneShot(laserSound);
    }

    private void KeepInsideCameraBounds()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        Vector3 p = body.position;
        p.x = Mathf.Clamp(p.x, -halfWidth + 0.35f, halfWidth - 0.35f);
        p.y = Mathf.Clamp(p.y, -halfHeight + 0.45f, halfHeight - 0.45f);
        body.position = p;
    }
}
