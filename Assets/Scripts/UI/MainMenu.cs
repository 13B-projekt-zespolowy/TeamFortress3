using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu functionality including starting the game and quitting the application.
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Loads the main game scene.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("ProstaMapa_Sprint1");
    }

    /// <summary>
    /// Quits the game application.
    /// In the Unity Editor, stops play mode instead.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else        
        Application.Quit();
#endif
    }
}
