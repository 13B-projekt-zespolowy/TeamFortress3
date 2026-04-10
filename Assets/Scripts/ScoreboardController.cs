using UnityEngine;
using UnityEngine.InputSystem; 

public class ScoreboardController : MonoBehaviour
{
    public GameObject scoreboardPanel;

    void Start()
    {
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            bool isTabHeld = Keyboard.current.tabKey.isPressed;
            scoreboardPanel.SetActive(isTabHeld);
        }
    }
}