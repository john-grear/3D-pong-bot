using UnityEngine;
using UnityEngine.EventSystems;

public class SceneManager : MonoBehaviour
{
    public GameObject playButton;
    public GameObject initialDifficultyButton;
    public GameObject initialSettingsSlider;

    public GameObject menuButtons;
    public GameObject difficultyButtons;
    public GameObject settingsPanel;

    /// <summary>
    /// Toggles between displaying the difficulty buttons and the menu buttons.
    /// </summary>
    public void TogglePlayOptions()
    {
        // Deselect button
        EventSystem.current.SetSelectedGameObject(null);
        
        // Toggle which buttons being displayed
        menuButtons.SetActive(!menuButtons.activeSelf);
        difficultyButtons.SetActive(!difficultyButtons.activeSelf);

        // Update selected button depending on which buttons being toggled
        EventSystem.current.SetSelectedGameObject(menuButtons.activeSelf ? playButton : initialDifficultyButton);
    }

    /// <summary>
    /// Loads the scene to start the classic Pong game.
    /// </summary>
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Pong");
    }

    /// <summary>
    /// Enables / disables the settings panel, toggling menu buttons being displayed as well.
    /// </summary>
    public void ToggleSettingsPanel()
    {
        // Deselect button
        EventSystem.current.SetSelectedGameObject(null);
        
        // Toggle which buttons being displayed
        menuButtons.SetActive(!menuButtons.activeSelf);

        // Update selected button depending on if settings panel open or not
        EventSystem.current.SetSelectedGameObject(settingsPanel.activeSelf ? playButton : initialSettingsSlider);

        // Toggle settings panel after selecting button to not lose selected button
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    /// <summary>
    /// Stops the editor or quits the game.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}