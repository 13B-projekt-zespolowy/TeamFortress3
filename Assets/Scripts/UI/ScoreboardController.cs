using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages the scoreboard UI, displaying player statistics when the scoreboard key is held.
/// Currently uses mock data for demonstration purposes.
/// </summary>
public class ScoreboardController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference toggleScoreboardAction;

    [Header("UI")]
    public GameObject scoreboardPanel;
    public Transform content;
    public GameObject playerRowPrefab;

    private readonly List<GameObject> rows = new();

    private void Start()
    {
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(false);
        }
    }

    private void Update()
    {
        var isTabHeld = toggleScoreboardAction.action.IsInProgress();

        scoreboardPanel.SetActive(isTabHeld);

        if (isTabHeld)
        {
            UpdateScoreboard(GetPlayers());
        }
    }

    /// <summary>
    /// Updates the scoreboard with player data by creating or refreshing rows.
    /// </summary>
    /// <param name="players">The list of player data to display.</param>
    private void UpdateScoreboard(List<PlayerData> players)
    {
        foreach (var row in rows)
        {
            Destroy(row);
        }
        rows.Clear();

        foreach (var player in players)
        {
            GameObject obj = Instantiate(playerRowPrefab, content);

            TextMeshProUGUI[] texts = obj.GetComponentsInChildren<TextMeshProUGUI>();

            texts[0].text = player.name;
            texts[1].text = player.kills.ToString();
            texts[2].text = player.deaths.ToString();
            texts[3].text = player.assists.ToString();

            rows.Add(obj);
        }
    }

    /// <summary>
    /// Retrieves player data for the scoreboard.
    /// Currently returns mock data - replace with actual networked player data.
    /// </summary>
    /// <returns>A list of PlayerData objects.</returns>
    private static List<PlayerData> GetPlayers()
    {
        return new List<PlayerData>()
        {
            new() { name = "Player 1", kills = 4, deaths = 5, assists = 3 },
            new() { name = "Player 2", kills = 2, deaths = 1, assists = 6 },
            new() { name = "Player 3", kills = 7, deaths = 3, assists = 2 }
        };
    }
}

/// <summary>
/// Represents a player's statistics for the scoreboard.
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string name;
    public int kills;
    public int deaths;
    public int assists;
}
