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

    [Header("Scout Mode Settings")]
    [SerializeField] private Transform goalPoint;
    [SerializeField] private float goalRadius = 3.0f;

    [SerializeField] private float scoutAlertDelay = 1.5f;

    [Header("Clear Reward")]
    [SerializeField] private WeaponData clearRewardWeaponScout;
    [SerializeField] private WeaponData clearRewardWeaponAnnihilation;
    [SerializeField] private WeaponData infiniteRewardWeapon;
    [SerializeField] private int infiniteRewardScoreThreshold = 1000;

    private bool infiniteRewardGranted = false;

    public Vector3 PlayerPosition { get; private set; }
    private bool isCleared = false;

    private bool isPlayerSpotted = false;
    private bool isSpottingPending = false;
    private Coroutine scoutAlertCoroutine = null;

    private HashSet<EnemyBase> seeingScouts = new HashSet<EnemyBase>();
    public bool IsPlayerSpotted { get { return isPlayerSpotted; } }

    private int mode = 0;

    private ScoreManager scoreManager;
    public bool IsInfiniteStage => isInfiniteStage;

    public int CurrentStageIndex => currentStageIndex;

    private void Awake()
    {
        if (DataManager.Instance != null)
        {
            mode = DataManager.Instance.selectedMode;
            isInfiniteStage = (mode == 2);
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
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }
    private void Start()
    {
        if (scoreManager != null)
            scoreManager.SetMissionGuide(mode);
    }
    //Update is called once per frame
    void Update()
    {
        if (player == null)return;

        PlayerPosition=player.position;

        if (isInfiniteStage)
        {
            CheckInfiniteReward();
        }

        if (!isCleared && !isInfiniteStage)
        {
            if (mode == 0) //정찰 모드
            {
                CheckGoalClear();
            }
            else if (mode == 1) //섬멸 모드
            {
                CheckStageClear();
            }
        }
    }

    private void CheckGoalClear()
    {
        if (goalPoint == null) return;

        float dist = Vector3.Distance(player.position, goalPoint.position);
        if (dist <= goalRadius)
        {
            StartCoroutine(ClearSequence());
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
            DataManager.Instance.ClearMission(currentStageIndex, mode);

            if (mode == 0)
            {
                clearRewardWeaponScout.isUnlocked = true;

                DataManager.Instance.UnlockWeapon(clearRewardWeaponScout.weaponName);
            }
            else if (mode == 1)
            {
                clearRewardWeaponAnnihilation.isUnlocked = true;

                DataManager.Instance.UnlockWeapon(clearRewardWeaponAnnihilation.weaponName);
            }
        }

        if (scoreManager != null && mode == 0) scoreManager.SetWarning(false);

        yield return new WaitForSeconds(2.0f); //연출을 위한 대기

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (scoreManager != null)
        {
            scoreManager.ShowGameOver(true);
        }
    }

    public Transform GetPlayerTransform()
    { 
        return player;
    }

    public void PlayerSpotted(Vector3 lastPosition)
    {
        if (isPlayerSpotted) return;
        isPlayerSpotted = true;
    }

    public void SetScoutSeeing(EnemyBase scout, bool isSeeing)
    {
        if (mode == 2) return;

        if (isPlayerSpotted) return;
        if (scout == null) return;

        if (isSeeing)
        {
            seeingScouts.Add(scout);

            if (scoreManager != null && mode == 0)
                scoreManager.SetWarning(true, "Spotted! Eliminate the scout or break line of sight!");

            if (!isSpottingPending)
            {
                isSpottingPending = true;
                scoutAlertCoroutine = StartCoroutine(CoScoutAlertDelay());
            }
        }
        else
        {
            seeingScouts.Remove(scout);

            if (seeingScouts.Count <= 0)
            {
                if (scoreManager != null && mode == 0)
                    scoreManager.SetWarning(false);

                CancelScoutAlert();
            }
        }
    }

    public void NotifyScoutDied(EnemyBase scout)
    {
        SetScoutSeeing(scout, false);
    }

    private IEnumerator CoScoutAlertDelay()
    {
        if (scoutAlertDelay > 0f)
            yield return new WaitForSeconds(scoutAlertDelay);

        if (!isPlayerSpotted && seeingScouts.Count > 0)
        {
            isPlayerSpotted = true;

            if (scoreManager != null && mode == 0)
                scoreManager.SetWarning(true, "Detected! All enemies are converging on your position!");
        }

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

    private void CheckInfiniteReward()
    {
        if (infiniteRewardGranted) return;

        if (DataManager.TotalScore >= infiniteRewardScoreThreshold)
        {
            infiniteRewardGranted = true;

            infiniteRewardWeapon.isUnlocked = true;

            if (DataManager.Instance != null)
                DataManager.Instance.UnlockWeapon(infiniteRewardWeapon.weaponName);
        }
    }

    private void OnDrawGizmos()
    {
        if (goalPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goalPoint.position, goalRadius);
        }
    }
}
