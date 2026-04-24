using PurrNet;
using UnityEngine;
using UnityEngine.Events;

public class GameTimer : NetworkBehaviour
{
    [Header("Timer settings")]
    [SerializeField] private SyncVar<float> timeRemaining = new(300.0f);

    [SerializeField] private bool timerIsRunning = true;

    public float TimeRemaining => timeRemaining;

    public UnityEvent OnTimerEnd;

    private ModeManager modeManager;

    private void Start()
    {
        modeManager = FindAnyObjectByType<ModeManager>();

        if (!isServer)
            return;

        timerIsRunning = true;
    }

    private void FixedUpdate()
    {
        if (!isServer)
            return;

        if (!timerIsRunning)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining.value -= Time.fixedDeltaTime;
        }
        else
        {
            timeRemaining.value = 0;
            StopTimer();
            modeManager.EndDraw();
        }
    }

    [ServerRpc]
    public void AddTime(float time)
    {
        if (!isServer)
            return;

        timeRemaining.value += time;
    }

    [ServerRpc]
    public void StopTimer()
    {
        if (!isServer)
            return;

        timerIsRunning = false;
    }
}
