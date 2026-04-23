using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public PlayerController playerMovementScript;

    private bool isMenuOpen = false;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (isMenuOpen)
            {
                ResumeGame();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    public void OpenMenu()
    {
        pauseMenuPanel.SetActive(true);
        isMenuOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovementScript != null) playerMovementScript.canMove = false;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        isMenuOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovementScript != null) playerMovementScript.canMove = true;
    }

    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
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
        else if (fpsIndex == 2) Application.targetFrameRate = 120;
        else if (fpsIndex == 3) Application.targetFrameRate = -1;
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ChangeTeam() { Debug.Log("Zmieniam dru¿ynê"); }
    public void ChangeClass() { Debug.Log("Zmieniam klasê"); }
    public void CallVote() { Debug.Log("Rozpoczynam g³osowanie"); }
}