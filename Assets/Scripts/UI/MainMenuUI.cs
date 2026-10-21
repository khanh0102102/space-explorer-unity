using UnityEngine;
using UnityEngine.UI;

/// <summary>Điều khiển các nút ở màn hình menu chính.</summary>
public sealed class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject instructionsPanel;

    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(GameManager.StartNewGame);
        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(ShowInstructions);
        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.AddListener(HideInstructions);
        if (quitButton != null)
            quitButton.onClick.AddListener(GameManager.QuitGame);

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }
}
