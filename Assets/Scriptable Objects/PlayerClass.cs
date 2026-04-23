using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClass", menuName = "Scriptable Objects/Player Class")]
public class PlayerClass : ScriptableObject
{
    public string className;

    [Header("Prefab")]
    public GameObject playerPrefab;

    [Header("Stats")]
    public int maxHealth;
}
