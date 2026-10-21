using UnityEngine;

/// <summary>Sao thưởng: tàu chạm vào sẽ nhận điểm và sao biến mất.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class StarCollectible : MonoBehaviour
{
    [SerializeField] private int points = 10;
    [SerializeField, Min(0f)] private float bobHeight = 0.12f;
    [SerializeField, Min(0.1f)] private float bobSpeed = 2.1f;
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;
    private bool collected;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        startPosition = transform.position;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed + transform.position.x) * bobHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + offset, startPosition.z);
        transform.Rotate(0f, 0f, 22f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponent<PlayerController>() == null)
            return;

        collected = true;
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(points);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject);
    }
}
