using PurrNet;
using UnityEngine;

public class PlayerTeam : MonoBehaviour
{
    [SerializeField] private SyncVar<Team> team;

    public Team Team => team;
}
