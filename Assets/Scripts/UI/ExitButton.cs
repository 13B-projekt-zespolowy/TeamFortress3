using UnityEngine;

public class ExitButton : MonoBehaviour
{
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
