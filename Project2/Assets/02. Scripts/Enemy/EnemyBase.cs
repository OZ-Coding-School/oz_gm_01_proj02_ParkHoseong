using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    [Header("Attack Cool")]
    [SerializeField] private float attackCool = 1.0f;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 30f;

    private NavMeshAgent agent;
    
    private int currentHp;
    private bool isDead = false;
    private bool canSeePlayer = false;
    private float lastAttackTime;

    private EnemyManager enemyManager;
    private BulletManager bulletManager;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        //일반 Stage->Manager자식으로 적들 넣기
        //Infinite Stage->Manager 자식으로 스포너 넣기
        enemyManager = GetComponentInParent<EnemyManager>();
        currentHp = data.baseHp;
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

        UpdateSight();
        HandleChase();
    }

    // 시야 판정
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

    // 추격 / 공격 거리 제어
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

    // 공격 처리
    void AttackMelee()
    {
        if (Time.time < lastAttackTime + attackCool)
            return;

        lastAttackTime = Time.time;

        PlayerHealth playerHealth = enemyManager.GetPlayerTransform().GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(data.meleeDamage);
            Debug.Log($"{data.enemyName} 근접 공격 성공!");
        }
    }

    void AttackRanged()
    {
        if (Time.time < lastAttackTime + attackCool)
            return;

        if (bulletManager == null || firePoint == null)
            return;

        lastAttackTime = Time.time;

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

    // 체력 관리
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp -= amount;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        Debug.Log($"{data.enemyName} 사망");
        gameObject.SetActive(false);
    }

    // -------------------------------
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
    }
}