using PurrNet;
using System;
using UnityEngine;

public class PlayerTeam : NetworkBehaviour
{
    [SerializeField] private SyncVar<Team> team;

    public Team Team {
        get => team.value;
        set => team.value = value;
    }

    public void InitializeTeam(Team assignedTeam)
    {
        if (!isServer) return;
        team.value = assignedTeam;
    }
}
