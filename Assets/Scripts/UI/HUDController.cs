using UnityEngine;
using UnityEngine.UI;

/// <summary>Hiển thị điểm và lời nhắc điều khiển ở màn Gameplay.</summary>
public sealed class HUDController : MonoBehaviour
{
    [SerializeField] private Text scoreText;

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "ĐIỂM: " + score.ToString("N0");
    }
}
