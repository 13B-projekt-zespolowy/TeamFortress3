using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer settings")]
    [SerializeField] private float timeRemaining = 300f;
    [SerializeField] private bool timerIsRunning = false;

    [Header("UI Element")]
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private ModeManager modeManager;

    public UnityEvent OnTimerEnd;

    private void Start()
    {
        if (timerText == null)
            Debug.LogError($"{nameof(timerText)} is null", this);

        timerIsRunning = true;
        DisplayTime(timeRemaining);
    }

    private void Update()
    {
        if (!timerIsRunning) return;

        if(timeRemaining > 0)
        {
            DisplayTime(timeRemaining);
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
            StopTimer();
            DisplayTime(timeRemaining);
            modeManager.EndDraw();
        }
    }

    public void AddTime(float time)
    {
        timeRemaining += time;
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }

    private void DisplayTime(float timeToDisplay)
    {
        if(timeRemaining < 10f)
        {
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
        }

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
