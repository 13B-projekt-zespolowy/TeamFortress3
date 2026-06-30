using UnityEngine;

/// <summary>
/// Manages the settings menu UI and applies player preferences such as volume, fullscreen, and frame rate.
/// Handles navigation between main menu and settings panels.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    /// <summary>
    /// Opens the settings panel and hides the main menu.
    /// </summary>
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the settings panel and returns to the main menu.
    /// </summary>
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Sets the master audio volume.
    /// </summary>
    /// <param name="volume">Volume value between 0 and 1.</param>
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; 
    }

    /// <summary>
    /// Sets the fullscreen mode.
    /// </summary>
    /// <param name="isFullscreen">Whether the game should be fullscreen.</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    /// <summary>
    /// Sets the target frame rate based on selected index.
    /// </summary>
    /// <param name="fpsIndex">0=30, 1=60, 2=90, 3=120, 4=144, 5=Unlimited.</param>
    public void SetFPS(int fpsIndex)
    {
        if (fpsIndex == 0) Application.targetFrameRate = 30;
        else if (fpsIndex == 1) Application.targetFrameRate = 60;
        else if (fpsIndex == 2) Application.targetFrameRate = 90;
        else if (fpsIndex == 3) Application.targetFrameRate = 120;
        else if (fpsIndex == 4) Application.targetFrameRate = 144;
        else if (fpsIndex == 5) Application.targetFrameRate = -1; 
    }
}
