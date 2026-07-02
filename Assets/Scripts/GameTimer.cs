using PurrNet;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the game timer, counting down from a specified duration.
/// Triggers events when the timer ends and supports adding time dynamically.
/// Server-authoritative with network synchronization.
/// </summary>
public class GameTimer : NetworkBehaviour
{
    [Header("Timer settings")]
    [SerializeField] private SyncVar<float> timeRemaining = new(300.0f);

    [SerializeField] private SyncVar<bool> timerIsRunning = new();

    /// <summary>
    /// Gets the current remaining time on the timer.
    /// </summary>
    public float TimeRemaining => timeRemaining;

    public bool IsRunning => timerIsRunning;

    /// <summary>
    /// Event invoked when the timer reaches zero.
    /// </summary>
    public UnityEvent OnTimerEnd;

    private ModeManager modeManager;

    private void Start()
    {
        modeManager = FindAnyObjectByType<ModeManager>();
        timerIsRunning.value = false;
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

    /// <summary>
    /// Adds additional time to the timer.
    /// Server-only operation.
    /// </summary>
    /// <param name="time">The amount of time to add in seconds.</param>
    [ServerRpc]
    public void AddTime(float time)
    {
        if (!isServer)
            return;

        timeRemaining.value += time;
    }

    /// <summary>
    /// Starts the timer.
    /// Server-only operation.
    /// </summary>
    [ServerRpc]
    public void StartTimer()
    {
        if (!isServer)
            return;

        timerIsRunning.value = true;
    }

    /// <summary>
    /// Stops the timer.
    /// Server-only operation.
    /// </summary>
    [ServerRpc]
    public void StopTimer()
    {
        if (!isServer)
            return;

        timerIsRunning.value = false;
    }
}
