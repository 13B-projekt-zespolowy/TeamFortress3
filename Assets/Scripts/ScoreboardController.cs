using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ScoreboardController : MonoBehaviour
{
    [Header("UI")]
    public GameObject scoreboardPanel;
    public Transform content;
    public GameObject playerRowPrefab;

    private List<GameObject> rows = new List<GameObject>();

    void Start()
    {
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        bool isTabHeld = Keyboard.current.tabKey.isPressed;

        scoreboardPanel.SetActive(isTabHeld);

        if (isTabHeld)
        {
            UpdateScoreboard(GetPlayers());
        }
    }

    void UpdateScoreboard(List<PlayerData> players)
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

    List<PlayerData> GetPlayers()
    {
        return new List<PlayerData>()
        {
            new PlayerData { name = "Gracz 1", kills = 4, deaths = 5, assists = 3 },
            new PlayerData { name = "Gracz 2", kills = 2, deaths = 1, assists = 6 },
            new PlayerData { name = "Gracz 3", kills = 7, deaths = 3, assists = 2 }
        };
    }
}

[System.Serializable]
public class PlayerData
{
    public string name;
    public int kills;
    public int deaths;
    public int assists;
}