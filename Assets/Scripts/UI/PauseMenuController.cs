using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the pause menu functionality including opening, closing, settings, and input mode switching.
/// Handles dimming of gameplay UI elements when the menu is open.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference togglePauseAction;

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Gameplay Elements To Dim")]
    public GameObject[] gameplayUIElements;
    public float dimmedAlpha = 0.4f;

    private bool isMenuOpen = false;

    private void Awake()
    {
        togglePauseAction.action.performed += _ => ToggleMenu();
        pauseMenuPanel.SetActive(false);
    }

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Toggles the pause menu based on current state and settings panel visibility.
    /// </summary>
    private void ToggleMenu()
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

    /// <summary>
    /// Opens the pause menu and dims gameplay UI elements.
    /// </summary>
    public void OpenMenu()
    {
        pauseMenuPanel.SetActive(true);
        isMenuOpen = true;

        pauseMenuPanel.transform.SetAsLastSibling();
        if (settingsPanel != null) settingsPanel.transform.SetAsLastSibling();

        foreach (GameObject ui in gameplayUIElements)
        {
            if (ui != null)
            {
                CanvasGroup cg = ui.GetComponent<CanvasGroup>();
                if (cg == null) cg = ui.AddComponent<CanvasGroup>();
                cg.alpha = dimmedAlpha;
            }
        }

        InputManager.Instance.SwitchInputMode(InputMode.Ui);
    }

    /// <summary>
    /// Resumes the game by closing the pause menu and restoring UI alpha.
    /// </summary>
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        isMenuOpen = false;

        foreach (GameObject ui in gameplayUIElements)
        {
            if (ui != null)
            {
                CanvasGroup cg = ui.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }

        InputManager.Instance.SwitchInputMode(InputMode.Gameplay);
    }

    /// <summary>
    /// Opens the settings panel and hides the pause menu.
    /// </summary>
    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the settings panel and returns to the pause menu.
    /// </summary>
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
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
    /// <param name="fpsIndex">0=30, 1=60, 2=120, 3=Unlimited.</param>
    public void SetFPS(int fpsIndex)
    {
        if (fpsIndex == 0) Application.targetFrameRate = 30;
        else if (fpsIndex == 1) Application.targetFrameRate = 60;
        else if (fpsIndex == 2) Application.targetFrameRate = 120;
        else if (fpsIndex == 3) Application.targetFrameRate = -1;
    }

    /// <summary>
    /// Quits to the main menu scene.
    /// </summary>
    public void QuitToMenu()
    {
        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// </summary>
    public void ChangeTeam() { Debug.Log("Changing team."); }

    /// <summary>
    /// </summary>
    public void ChangeClass() { Debug.Log("Changing class."); }

    /// <summary>
    /// </summary>
    public void CallVote() { Debug.Log("Calling vote."); }
}
