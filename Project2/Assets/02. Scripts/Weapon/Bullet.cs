using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

public class Bullet : MonoBehaviour
{
    public enum BulletOwner { Player, Enemy }
    public BulletOwner owner;

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] public ScoreManager scoreManager;

    [SerializeField] private float maxLife=3.0f;
    private float lifeTimer;
    public int baseDamage = 40;
    private Rigidbody bulletRigid;

    private void Awake()
    {
        bulletRigid = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        lifeTimer = 0f;

        if (bulletRigid != null)
        {
            bulletRigid.velocity = Vector3.zero;
            bulletRigid.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLife)
        {
            ReturnPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            ReturnPool();
            return;
        }
        HealthBase targetHealth = other.GetComponentInParent<HealthBase>();
        if (targetHealth != null)
        {
            //피아식별
            bool isPlayerHitByPlayer = (owner == BulletOwner.Player && other.tag.Contains("Player"));
            bool isEnemyHitByEnemy = (owner == BulletOwner.Enemy && other.tag.Contains("Enemy"));

            if (isPlayerHitByPlayer || isEnemyHitByEnemy) return;
            int finalDamage = baseDamage;
            bool isHeadShot = other.CompareTag("EnemyHead") || other.CompareTag("PlayerHead");

            if (isHeadShot)
            {
                finalDamage *= 3;
            }
            targetHealth.TakeDamage(finalDamage, isHeadShot);

            ReturnPool();
        }
    }

    //총알이 발사될 때 지정된 방향과 속도를 리지드바디에 적용해서 실제로 날아가게 함
    public void Shot(Vector3 dir, float speed)
    {
        moveSpeed = speed;
        //rigid의 속도 설정
        bulletRigid.velocity = dir * moveSpeed;
    }

    private void ReturnPool() 
    {
        gameObject.SetActive(false);
    }
}
