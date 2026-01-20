using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Default Enemy";
    public GameObject prefab;

    [Header("Stat Settings")]
    public int baseHp = 100;
    public float baseMoveSpeed = 3.5f;
    public float findRange = 15f;
    public float attackRange = 10f;
    public int meleeDamage = 0;

    [Header("Sight Settings")]
    [Range(10, 180)] public float fieldOfView = 60f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;
    public string playerTag = "Player";

    [Header("AI Type")]
    public bool isRanged = false;
    public bool isMelee = false;
    public bool isScout = false;
}
