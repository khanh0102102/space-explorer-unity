using UnityEngine;
using UnityEngine.UI;

/// <summary>Hiển thị điểm cuối, kỷ lục và điều hướng sau màn chơi.</summary>
public sealed class EndGameUI : MonoBehaviour
{
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text bestScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (finalScoreText != null)
            finalScoreText.text = "ĐIỂM CỦA BẠN: " + GameManager.FinalScore.ToString("N0");
        if (bestScoreText != null)
            bestScoreText.text = "KỶ LỤC: " + GameManager.BestScore.ToString("N0");

        if (retryButton != null)
            retryButton.onClick.AddListener(GameManager.StartNewGame);
        if (menuButton != null)
            menuButton.onClick.AddListener(GameManager.ReturnToMenu);
        if (quitButton != null)
            quitButton.onClick.AddListener(GameManager.QuitGame);
    }
}
