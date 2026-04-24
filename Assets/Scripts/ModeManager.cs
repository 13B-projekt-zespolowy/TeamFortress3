using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ModeManager : MonoBehaviour
{
    public static ModeManager Instance;

    private int redScore;
    private int blueScore;
    [SerializeField] private int winScore = 3;

    [SerializeField] private GameTimer timer;

    [SerializeField] private TextMeshProUGUI redText;
    [SerializeField] private TextMeshProUGUI blueText;
    [SerializeField] private TextMeshProUGUI resultText;

    public UnityEvent onGameEnd;

    private void Awake()
    {
        Instance = this;
    }

    public void Score(Team team)
    {
        if (team == Team.Red)
            redScore++;
        else if (team == Team.Blue)
            blueScore++;

        timer.AddTime(180f);

        redText.text = redScore.ToString();
        blueText.text = blueScore.ToString();

        if (redScore >= winScore)
            EndWin(Team.Red);
        if (blueScore >= winScore)
            EndWin(Team.Blue);
    }

    public void EndWin(Team winner)
    {
        resultText.text = $"Winner: {winner}";
        onGameEnd.Invoke();
    }

    public void EndDraw()
    {
        resultText.text = "Draw";
        onGameEnd.Invoke();
    }
}

public enum Team
{
    Red,
    Blue
}