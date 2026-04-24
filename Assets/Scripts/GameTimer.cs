using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [Header("Timer settings")]
    [SerializeField] private float timeRemaining = 300f;
    [SerializeField] private bool timerIsRunning = false;

    [SerializeField] private ModeManager modeManager;

    public float TimeRemaining => timeRemaining;

    public UnityEvent OnTimerEnd;

    private void Start()
    {
        timerIsRunning = true;
    }

    private void FixedUpdate()
    {
        if (!timerIsRunning)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.fixedDeltaTime;
        }
        else
        {
            timeRemaining = 0;
            StopTimer();
            if (modeManager != null)
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
}
