using UnityEngine;
using UnityEngine.EventSystems;

public class SceneManager : MonoBehaviour
{
    public GameObject title;
    public GameObject subtitle;

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
    /// Enable the title, subtitle, and all buttons associated.
    /// </summary>
    public void EnableMenu()
    {
        title.SetActive(true);
        subtitle.SetActive(true);
        menuButtons.SetActive(true);
        difficultyButtons.SetActive(true);
    }

    /// <summary>
    /// Disable the title, subtitle, and all buttons associated.
    /// </summary>
    public void DisableMenu()
    {
        title.SetActive(false);
        subtitle.SetActive(false);
        menuButtons.SetActive(false);
        difficultyButtons.SetActive(false);
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