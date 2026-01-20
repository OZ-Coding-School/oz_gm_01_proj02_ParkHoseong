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

    [SerializeField] private float scoutAlertDelay = 1.5f;

    public Vector3 PlayerPosition { get; private set; }
    private bool isCleared = false;

    private bool isPlayerSpotted = false;
    private bool isSpottingPending = false;
    private Coroutine scoutAlertCoroutine = null;

    private HashSet<EnemyBase> seeingScouts = new HashSet<EnemyBase>();
    public bool IsPlayerSpotted { get { return isPlayerSpotted; } }


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

    public void PlayerSpotted()
    {
        if (isPlayerSpotted) return;
        isPlayerSpotted = true;
    }

    public void SetScoutSeeing(EnemyBase scout, bool isSeeing)
    {
        if (isPlayerSpotted) return;
        if (scout == null) return;

        if (isSeeing)
        {
            seeingScouts.Add(scout);

            // Start delay only once
            if (!isSpottingPending)
            {
                isSpottingPending = true;
                scoutAlertCoroutine = StartCoroutine(CoScoutAlertDelay());
            }
        }
        else
        {
            if (seeingScouts.Contains(scout)) seeingScouts.Remove(scout);

            // If no scout is seeing player anymore, cancel pending alert
            if (seeingScouts.Count <= 0)
            {
                CancelScoutAlert();
            }
        }
    }

    public void NotifyScoutDied(EnemyBase scout)
    {
        SetScoutSeeing(scout, false);
    }

    public void RaisePlayerSpottedDelayed()
    {
        if (isPlayerSpotted || isSpottingPending) return;
        if (seeingScouts.Count <= 0) return;

        if (!isSpottingPending)
        {
            isSpottingPending = true;
            scoutAlertCoroutine = StartCoroutine(CoScoutAlertDelay());
        }
    }

    private IEnumerator CoScoutAlertDelay()
    {
        if (scoutAlertDelay > 0f)
            yield return new WaitForSeconds(scoutAlertDelay);

        if (!isPlayerSpotted && seeingScouts.Count > 0)
            isPlayerSpotted = true;

        isSpottingPending = false;
        scoutAlertCoroutine = null;
    }

    private void CancelScoutAlert()
    {
        if (!isSpottingPending) return;

        if (scoutAlertCoroutine != null)
        {
            StopCoroutine(scoutAlertCoroutine);
            scoutAlertCoroutine = null;
        }

        isSpottingPending = false;
    }
}
