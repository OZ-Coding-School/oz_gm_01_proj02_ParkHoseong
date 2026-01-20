using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Stage Settings")]
    [SerializeField] private int currentStageIndex;
    [SerializeField] public bool isInfiniteStage = false;

    public Vector3 PlayerPosition { get; private set; }
    private bool isCleared = false;

    private void Awake()
    {
        if (DataManager.Instance != null)
        {
            isInfiniteStage = DataManager.Instance.isInfiniteMode;
        }

        EnemySpawner[] spawners = GetComponentsInChildren<EnemySpawner>(true);

        foreach (var spawner in spawners)
        {
            spawner.gameObject.SetActive(isInfiniteStage);
        }

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

        if (!isInfiniteStage && !isCleared)
        {
            CheckStageClear();
        }
    }

    private void CheckStageClear()
    {
        EnemyBase[] aliveEnemies = GetComponentsInChildren<EnemyBase>(false);

        if (aliveEnemies.Length <= 0)
        {
            StartCoroutine(ClearSequence());
        }
    }
    private IEnumerator ClearSequence()
    {
        isCleared = true;
        Debug.Log("모든 적 처치 완료!");

        if (DataManager.Instance != null)
        {
            DataManager.Instance.ClearStage(currentStageIndex);
            DataManager.Instance.Save();
        }

        yield return new WaitForSeconds(2.0f); //연출을 위한 대기

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        if (sm != null)
        {
            sm.ShowGameOver(true);
        }
    }

    public Transform GetPlayerTransform()
    { 
        return player;
    }
}
