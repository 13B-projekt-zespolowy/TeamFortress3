using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Ustawienia Timera")]
    [SerializeField] private float timeRemaining = 300f;
    [SerializeField] private bool timerIsRunning = false;

    [Header("UI Element")]
    [SerializeField] private TextMeshProUGUI timerText;

    public UnityEvent OnTimerEnd;

    private void Start()
    {
        if (timerText == null)
            Debug.LogError("Nie przypisano timerText", this);

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
            timerIsRunning = false;
            DisplayTime(timeRemaining);
            OnTimerEnd.Invoke();
        }
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
