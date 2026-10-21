using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý điểm số và luồng kết thúc màn chơi.
/// Đặt đúng một GameManager trong scene Gameplay.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private HUDController hud;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField, Min(0f)] private float endScreenDelay = 0.8f;

    public int CurrentScore { get; private set; }
    public bool IsGameOver { get; private set; }

    private const string FinalScoreKey = "SpaceExplorer_FinalScore";
    private const string BestScoreKey = "SpaceExplorer_BestScore";

    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
    public static int FinalScore => PlayerPrefs.GetInt(FinalScoreKey, 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        CurrentScore = 0;
        IsGameOver = false;
        RefreshHud();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Thay đổi điểm và đồng bộ ngay lên HUD.</summary>
    public void AddScore(int amount)
    {
        if (IsGameOver)
            return;

        CurrentScore = Mathf.Max(0, CurrentScore + amount);
        RefreshHud();
    }

    /// <summary>Được gọi khi tàu đâm vào thiên thạch.</summary>
    public void PlayerCrashed()
    {
        if (IsGameOver)
            return;

        AddScore(-10);
        IsGameOver = true;

        PlayerPrefs.SetInt(FinalScoreKey, CurrentScore);
        if (CurrentScore > BestScore)
            PlayerPrefs.SetInt(BestScoreKey, CurrentScore);
        PlayerPrefs.Save();

        if (gameOverClip != null)
            AudioSource.PlayClipAtPoint(gameOverClip, Vector3.zero);

        StartCoroutine(LoadEndGameAfterDelay());
    }

    private IEnumerator LoadEndGameAfterDelay()
    {
        // Dùng thời gian thực để chuyển cảnh vẫn hoạt động khi tạm dừng vật lý.
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(endScreenDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene("EndGame");
    }

    private void RefreshHud()
    {
        if (hud != null)
            hud.SetScore(CurrentScore);
    }

    public static void StartNewGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay");
    }

    public static void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Thoát game: lệnh này chỉ đóng ứng dụng trong bản build.");
#else
        Application.Quit();
#endif
    }
}
