using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button backButton;

    public GameObject settingsPanel;

    /// <summary>
    /// Toggles between displaying the easy, medium, and hard modes to choose which one to play and
    /// the start, settings, and quit buttons.
    /// </summary>
    public void TogglePlayOptions()
    {
        startButton.gameObject.SetActive(!startButton.gameObject.activeSelf);
        settingsButton.gameObject.SetActive(!settingsButton.gameObject.activeSelf);
        quitButton.gameObject.SetActive(!quitButton.gameObject.activeSelf);

        easyButton.gameObject.SetActive(!easyButton.gameObject.activeSelf);
        mediumButton.gameObject.SetActive(!mediumButton.gameObject.activeSelf);
        hardButton.gameObject.SetActive(!hardButton.gameObject.activeSelf);
        backButton.gameObject.SetActive(!backButton.gameObject.activeSelf);
    }

    /// <summary>
    /// Loads the scene to start the classic Pong game.
    /// </summary>
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Pong");
    }

    /// <summary>
    /// Enables / disables the settings panel.
    /// </summary>
    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}