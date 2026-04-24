using PurrNet;
using UnityEngine;

public class FlagBase : NetworkBehaviour
{
    [SerializeField] private Team team;
    public Team Team => team;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.CompareTag("Player"))
        {
            var flag = other.GetComponent<PlayerFlagCarry>().carriedFlag;

            if (flag != null && flag.Team != team)
            {
                ModeManager.Instance.IncreaseScore(flag.Team);
                flag.ReturnToBase();
            }
        }
    }
}