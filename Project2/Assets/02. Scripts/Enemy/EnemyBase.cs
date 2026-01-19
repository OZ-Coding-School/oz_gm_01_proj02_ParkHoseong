using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : HealthBase
{
    [SerializeField] private EnemyData data;

    [Header("Attack Cool")]
    [SerializeField] private float attackCool = 1.0f;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 30f;

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

        agent.speed = data.baseMoveSpeed;
    }

    void Update()
    {
        if (isDead) return;

        anim.SetFloat("Speed", agent.velocity.magnitude);

        UpdateSight();
        HandleChase();
    }

    //시야 판정
    void UpdateSight()
    {
        canSeePlayer = false;

        Vector3 dirToPlayer = (enemyManager.PlayerPosition - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, enemyManager.PlayerPosition);

        if (distToPlayer > data.findRange)
            return;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > data.fieldOfView * 0.5f)
            return;

        if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, distToPlayer, data.obstacleMask))
        {
            canSeePlayer = true;
        }
    }

    //추격/공격 거리 제어
    void HandleChase()
    {
        float dist = Vector3.Distance(transform.position, enemyManager.PlayerPosition);

        if (canSeePlayer)
        {
            if (dist > data.attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(enemyManager.PlayerPosition);
            }
            else
            {
                agent.isStopped = true;

                if (data.isMelee)
                    AttackMelee();
                else if (data.isRanged)
                    AttackRanged();
            }
        }
        else
        {
            agent.isStopped = true;
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

        if (bulletManager == null || firePoint == null)
            return;

        lastAttackTime = Time.time;

        anim.SetTrigger("Attack");

        Vector3 dir = (enemyManager.PlayerPosition - firePoint.position).normalized;

        GameObject bulletObj = bulletManager.GetBulletPrefab();
        if (bulletObj == null)
            return;

        bulletObj.transform.position = firePoint.position;
        bulletObj.transform.rotation = Quaternion.LookRotation(dir);
        bulletObj.SetActive(true);

        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.owner = Bullet.BulletOwner.Enemy;
            bullet.Shot(dir, bulletSpeed);
        }

        Debug.Log($"{data.enemyName} 원거리 공격!");
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        int finalScore = isHeadShot ? 300 : 100;

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(finalScore);
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

    private IEnumerator SuicideSequence()
    {
        //자폭 연출을 위한 짧은 대기(0.5초)
        yield return new WaitForSeconds(0.5f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hitCollider in hitColliders)
        {
            HealthBase hb = hitCollider.GetComponentInParent<HealthBase>();

            if (hb != null)
            {
                //피아구분 없이 데미지 전달
                hb.TakeDamage(data.meleeDamage, false);
            }
        }
    }

    // 시야 디버그 표시
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