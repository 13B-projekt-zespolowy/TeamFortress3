using PurrNet;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the game mode logic including scoring, win conditions, and match results.
/// Handles network synchronization of scores and UI updates.
/// </summary>
public class ModeManager : NetworkBehaviour
{
    public static ModeManager Instance { get; private set; }

    private SyncVar<int> redScore = new();
    private SyncVar<int> blueScore = new();
    [SerializeField] private int winScore = 3;

    [SerializeField] private TextMeshProUGUI redText;
    [SerializeField] private TextMeshProUGUI blueText;
    [SerializeField] private TextMeshProUGUI resultText;

    private GameTimer timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        timer = FindAnyObjectByType<GameTimer>();
    }

    /// <summary>
    /// Increases the score for the specified team and checks for win conditions.
    /// Server-only operation.
    /// </summary>
    /// <param name="team">The team that scored.</param>
    public void IncreaseScore(Team team)
    {
        if (!isServer)
            return;

        if (team == Team.Red)
            redScore.value++;
        else if (team == Team.Blue)
            blueScore.value++;

        timer.AddTime(180f);

        redText.text = redScore.ToString();
        blueText.text = blueScore.ToString();

        if (redScore >= winScore)
            EndWin(Team.Red);
        if (blueScore >= winScore)
            EndWin(Team.Blue);
    }

    /// <summary>
    /// Ends the match with a win for the specified team.
    /// </summary>
    /// <param name="winner">The winning team.</param>
    public void EndWin(Team winner)
    {
        timer.StopTimer();
        ShowResultText($"Winner: {winner}");
    }

    /// <summary>
    /// Ends the match in a draw.
    /// </summary>
    public void EndDraw()
    {
        timer.StopTimer();
        ShowResultText("Draw");
    }

    /// <summary>
    /// Ends the match when the time runs out. Selects score based on team points.
    /// </summary>
    public void EndTimeout()
    {
        if (redScore > blueScore)
        {
            EndWin(Team.Red);
        }
        else if (blueScore > redScore)
        {
            EndWin(Team.Blue);
        }
        else
        {
            EndDraw();
        }
    }


    /// <summary>
    /// RPC that displays the result text to all clients.
    /// </summary>
    /// <param name="text">The text to display.</param>
    [ObserversRpc]
    private void ShowResultText(string text)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = text;
    }
}

/// <summary>
/// Defines the available teams in the game.
/// </summary>
public enum Team
{
    Red,
    Blue
}
