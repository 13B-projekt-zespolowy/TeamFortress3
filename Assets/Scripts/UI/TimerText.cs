using UnityEngine;
using TMPro;

/// <summary>
/// Displays the game timer in a formatted MM:SS format.
/// Changes text color to red when less than 10 seconds remain.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerText : MonoBehaviour
{
    private GameTimer timer;
    private TextMeshProUGUI text;

    private void Start()
    {
        timer = FindAnyObjectByType<GameTimer>();
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        var time = timer.TimeRemaining;
        text.color = time < 10.0f ? Color.red : Color.white;
        var minutes = (int)time / 60;
        var seconds = (int)time % 60;
        text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
