using UnityEngine;

public class Flag : MonoBehaviour
{
    public Team team;
    public Transform basePoint;

    private PlayerFlag carrier;

    public bool IsCarried => carrier != null;

    private void Update()
    {
        if (IsCarried) transform.position = carrier.holdPoint.position;
    }

    public void Pickup(PlayerFlag player)
    {
        carrier = player;
        player.carriedFlag = this;
        transform.SetParent(player.holdPoint);
        transform.localPosition = Vector3.zero;
    }

    public void Drop(Vector3 pos)
    {
        carrier = null;
        transform.SetParent(null);
        transform.position = pos;
    }

    public void ReturnToBase()
    {
        carrier = null;
        transform.SetParent(null);
        transform.position = basePoint.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerFlag>();
        if (player == null) return;

        if (player.team == team)
        {
            ReturnToBase();
            return;
        }

        if (!IsCarried && player.carriedFlag == null) Pickup(player);
    }
}