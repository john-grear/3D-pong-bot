using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button settingsQuitButton;
    public Button quitButton;
    public GameObject settingsPanel;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Disables settings panel by default and adds functionality to main menu buttons.
    /// </remarks>
    private void Start()
    {
        // Ensure settings panel is hidden at the start
        settingsPanel.SetActive(false);

        // Add listeners to buttons
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        settingsQuitButton.onClick.AddListener(ToggleSettingsPanel);
        quitButton.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// Loads the scene to start the classic Pong game.
    /// </summary>
    private static void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Pong");
    }

    /// <summary>
    /// Enables / disables the settings panel.
    /// </summary>
    private void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}