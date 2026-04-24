using PurrNet;
using UnityEngine;

public class Flag : NetworkBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private Transform basePosition;

    private PlayerFlagCarry carrier;

    private bool IsCarried => carrier != null;
    public Team Team => team;

    private void Awake()
    {
        if (!isServer)
            return;

        transform.position = basePosition.position;
    }

    public void Pickup(PlayerFlagCarry player)
    {
        if (!isServer)
            return;

        carrier = player;
        carrier.carriedFlag = this;
        transform.SetParent(player.holdPoint);
        transform.localPosition = Vector3.zero;
    }

    public void Drop(Vector3 pos)
    {
        if (!isServer)
            return;

        if (carrier != null)
            carrier.carriedFlag = null;
        carrier = null;
        transform.SetParent(null);
        transform.position = pos;
    }

    public void ReturnToBase()
    {
        if (!isServer)
            return;

        if (carrier != null)
            carrier.carriedFlag = null;
        carrier = null;
        transform.SetParent(null);
        transform.position = basePosition.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerFlagCarry>();
            var playerTeam = other.GetComponent<PlayerTeam>();

            if (playerTeam.Team == team)
            {
                ReturnToBase();
                return;
            }

            if (!IsCarried && player.carriedFlag == null)
                Pickup(player);
        }
    }
}