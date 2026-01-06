using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    public Vector3 PlayerPosition { get; private set; }

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            { 
                player = playerObj.transform;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (player == null)return;

        PlayerPosition=player.position;
    }

    public Transform GetPlayerTransform()
    { 
        return player;
    }
}
