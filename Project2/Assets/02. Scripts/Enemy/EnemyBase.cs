using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : HealthBase
{
    [SerializeField] private EnemyData data;

    [Header("Attack Cool")]
    [SerializeField] private float attackCool = 1.0f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("Patrol")]
    [SerializeField] private bool usePatrol = true;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolArriveDistance = 0.6f;
    [SerializeField] private float patrolWaitTime = 0.5f;
    [SerializeField] private float faceTurnSpeed = 12f;

    [Header("Explosion VFX")]
    [SerializeField] private ParticleSystem explosionFx;

    private WeaponBase weapon;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    private int patrolIndex = 0;
    private int patrolDir = 1; //1 or -1
    private float patrolWaitEndTime = -1f;

    private NavMeshAgent agent;
    
    private bool canSeePlayer = false;
    private float lastAttackTime;

    private EnemyManager enemyManager;
    private BulletManager bulletManager;
    private Animator anim;

    float explosionRadius = 5f;

    protected override void Awake()
    {
        if (data != null)
        {
            maxHealth = data.baseHp;
        }
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

        //일반 Stage->Manager자식으로 적들 넣기
        //Infinite Stage->Manager 자식으로 스포너 넣기
        enemyManager = GetComponentInParent<EnemyManager>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (bulletManager == null)
        {
            GameObject bmObj = GameObject.Find("EnemyBulletManager");
            if (bmObj != null)
                bulletManager = bmObj.GetComponent<BulletManager>();
        }

        if (data.isRanged)
        {
            weapon = weaponPrefab.GetComponent<WeaponBase>();
        }

        if (enemyManager.isInfiniteStage)
        {
            canSeePlayer = true;
        }
        else
        {
            StartCoroutine(SightUpdateRoutine());

            if (!data.isMelee && usePatrol && patrolPoints != null && patrolPoints.Length > 0)
            {
                patrolIndex = Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1);

                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(patrolPoints[patrolIndex].position);
                }
            }
        }

        agent.speed = data.baseMoveSpeed;
    }

    IEnumerator SightUpdateRoutine()
    {
        while (!isDead)
        {
            UpdateSight();
            yield return new WaitForSeconds(0.2f); // 0.2초마다 체크 (성능 최적화)
        }
    }

    void Update()
    {
        if (isDead) return;

        anim.SetFloat("Speed", agent.velocity.magnitude);

        HandleChase();
    }

    //시야 판정
    void UpdateSight()
    {
        bool wasSeeingPlayer = canSeePlayer;
        canSeePlayer = false;

        Vector3 targetPos = enemyManager.PlayerPosition;
        Vector3 dirToPlayer = (targetPos - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, targetPos);
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        bool inRange = distToPlayer <= data.findRange;
        bool inFov = angle <= data.fieldOfView * 0.5f;

        if (inRange)
        {
            bool hasLineOfSight = !Physics.Raycast(
                transform.position + Vector3.up * 1.5f,
                dirToPlayer,
                distToPlayer,
                data.obstacleMask
            );

            if (hasLineOfSight && (inFov || wasSeeingPlayer))
            {
                canSeePlayer = true;
            }
        }

        if (data.isScout && enemyManager != null)
            enemyManager.SetScoutSeeing(this, canSeePlayer);
    }

    //추격/공격 거리 제어
    void HandleChase()
    {
        float dist = Vector3.Distance(transform.position, enemyManager.PlayerPosition);

        bool seen = canSeePlayer || (enemyManager != null && enemyManager.IsPlayerSpotted);

        if (seen)
        {
            FacePlayer();

            if (dist > data.attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(enemyManager.PlayerPosition);
            }
            else
            {
                agent.isStopped = true;

                if (agent.isOnNavMesh) 
                    agent.ResetPath();

                if (data.isMelee)
                    AttackMelee();
                else if (data.isRanged)
                    AttackRanged();
            }
        }
        else
        {
            if (!enemyManager.isInfiniteStage
            && !data.isMelee
            && usePatrol
            && patrolPoints != null
            && patrolPoints.Length > 0)
            {
                UpdatePatrol();
            }
            else
            {
                agent.isStopped = true;

                if (agent.isOnNavMesh) 
                    agent.ResetPath();
            }
        }
    }

    private void FacePlayer()
    {
        Vector3 toPlayer = enemyManager.PlayerPosition - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * faceTurnSpeed);
    }

    private void UpdatePatrol()
    {
        if (!agent.isOnNavMesh) return;

        if (patrolWaitEndTime > 0f && Time.time < patrolWaitEndTime)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        agent.isStopped = false;

        Transform target = patrolPoints[Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1)];
        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(patrolArriveDistance, agent.stoppingDistance + 0.05f))
        {
            patrolWaitEndTime = Time.time + Mathf.Max(0f, patrolWaitTime);

            if (patrolPoints.Length >= 2)
            {
                int next = patrolIndex + patrolDir;

                if (next >= patrolPoints.Length)
                {
                    patrolDir = -1;
                    next = patrolIndex + patrolDir;
                }
                else if (next < 0)
                {
                    patrolDir = 1;
                    next = patrolIndex + patrolDir;
                }

                patrolIndex = Mathf.Clamp(next, 0, patrolPoints.Length - 1);
            }
        }
    }

    //공격 처리
    void AttackMelee()
    {
        if (Time.time < lastAttackTime + attackCool)
            return;

        lastAttackTime = Time.time;

        if (anim != null) anim.SetTrigger("Attack");

        PlayerHealth playerHealth = enemyManager.GetPlayerTransform().GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(data.meleeDamage);
            Debug.Log($"{data.enemyName} 근접(자폭) 공격 성공!");
        }
        if (data.enemyName.Contains("Bomb"))
        {
            StartCoroutine(SuicideSequence());
        }
    }

    void AttackRanged()
    {
        if (Time.time < lastAttackTime + attackCool)
            return;

        if (isReloading)
            return;

        if (weapon.currentAmmo <= 0)
        {
            if (weapon.currentTotalClips > 0)
            {
                StartReload();
            }
            return;
        }

        lastAttackTime = Time.time;

        anim.SetTrigger("Attack");

        Vector3 dir = (enemyManager.PlayerPosition - weapon.firePoint.position).normalized;

        GameObject bulletObj = bulletManager.GetBulletPrefab();

        if (bulletObj == null)
            return;

        bulletObj.transform.position = weapon.firePoint.position;
        bulletObj.transform.rotation = Quaternion.LookRotation(dir);
        bulletObj.SetActive(true);

        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.owner = Bullet.BulletOwner.Enemy;
            bullet.Shot(dir, weapon.weaponData.bulletSpeed);
        }

        weapon.currentAmmo--;

        Debug.Log($"{data.enemyName} 원거리 공격!");
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        if (data != null && data.isScout && enemyManager != null)
        {
            enemyManager.NotifyScoutDied(this);
        }

        int finalScore = isHeadShot ? 300 : 100;

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(finalScore);

            scoreManager.ShowHitMarker(isHeadShot);
        }

        if (agent != null) agent.isStopped = true;

        if (anim != null) anim.SetTrigger("Die");

        StartCoroutine(DisableAfterDelay(2.5f));

        Debug.Log($"{(isHeadShot ? "헤드샷! " : "")}{data.enemyName} 사살! {finalScore}점 획득");
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); //설정한 시간만큼 대기
        gameObject.SetActive(false); //시간이 지난 후 꺼짐
    }

    public void Setup()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (data == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyData(data)가 할당되지 않았습니다!");
            return;
        }

        maxHealth = data.baseHp;
        currentHealth = maxHealth;
        isDead = false;

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = data.baseMoveSpeed;
            if (agent.isOnNavMesh) agent.ResetPath();
        }

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
        if (TryGetComponent(out Collider col)) col.enabled = true;
    }

    private void StartReload()
    {
        if (isReloading)
            return;

        isReloading = true;

        weapon.currentTotalClips--;

        anim.SetTrigger("Reload");

        reloadCoroutine = StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(weapon.weaponData.reloadTime);

        weapon.currentAmmo = weapon.weaponData.maxAmmo;

        isReloading = false;

        reloadCoroutine = null;
    }

    private IEnumerator SuicideSequence()
    {
        //자폭 연출을 위한 짧은 대기(0.5초)
        yield return new WaitForSeconds(0.5f);

        explosionFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosionFx.Play(true);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        HashSet<HealthBase> targets = new HashSet<HealthBase>();

        foreach (var hitCollider in hitColliders)
        {
            HealthBase hb = hitCollider.GetComponentInParent<HealthBase>();

            if (hb != null && targets.Add(hb))
            {
                //피아구분없음
                hb.TakeDamage(data.bombDamage, false);
            }
        }
        Die();
    }

    private void OnDisable()
    {
        if (data != null && data.isScout && enemyManager != null)
        {
            enemyManager.SetScoutSeeing(this, false);
        }
    }

    //시야 디버그 표시
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = canSeePlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.findRange);

        Vector3 leftDir = Quaternion.Euler(0, -data.fieldOfView / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, data.fieldOfView / 2, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * data.findRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * data.findRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}