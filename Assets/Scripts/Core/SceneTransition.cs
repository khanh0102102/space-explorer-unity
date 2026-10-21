using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Chuyển scene bằng hiệu ứng phủ đen mờ dần. Dùng unscaledDeltaTime nên vẫn chạy
/// kể cả khi gameplay đang tạm dừng để xử lý Game Over.
/// </summary>
public sealed class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField, Min(0.05f)] private float fadeDuration = 0.4f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (!Instance.isTransitioning)
            Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        isTransitioning = true;
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = false;
            yield return FadeTo(1f);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
        yield return null;

        if (fadeGroup != null)
        {
            yield return FadeTo(0f);
            fadeGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeGroup == null)
            yield break;

        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }
}
