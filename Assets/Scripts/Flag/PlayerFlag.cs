using UnityEngine;

public class PlayerFlag : MonoBehaviour
{
    public Team team;
    public Transform holdPoint;

    public Flag carriedFlag;

    public void Drop(Vector3 pos)
    {
        if (carriedFlag == null) return;
        
        carriedFlag.Drop(pos);
        carriedFlag = null;
    }
}