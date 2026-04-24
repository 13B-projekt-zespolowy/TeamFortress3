using PurrNet;
using UnityEngine;
using TMPro;

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

    public void EndWin(Team winner)
    {
        timer.StopTimer();
        ShowResultText($"Winner: {winner}");
    }

    public void EndDraw()
    {
        timer.StopTimer();
        ShowResultText("Draw");
    }

    [ObserversRpc]
    private void ShowResultText(string text)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = text;
    }
}

public enum Team
{
    Red,
    Blue
}