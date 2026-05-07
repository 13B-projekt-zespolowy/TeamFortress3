using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference togglePauseAction;

    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

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

    public void OpenMenu()
    {
        pauseMenuPanel.SetActive(true);
        isMenuOpen = true;

        InputManager.Instance.SwitchInputMode(InputMode.Ui);
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        isMenuOpen = false;

        InputManager.Instance.SwitchInputMode(InputMode.Gameplay);
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

    public void ChangeTeam() { Debug.Log("Changing team."); }
    public void ChangeClass() { Debug.Log("Changing class."); }
    public void CallVote() { Debug.Log("Calling vote."); }
}