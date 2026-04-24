using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class SettingsMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; 
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

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

