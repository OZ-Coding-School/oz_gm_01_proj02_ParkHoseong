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
        if (owner == BulletOwner.Player && other.CompareTag("EnemyHead"))
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(baseDamage * 3);
                scoreManager?.AddScore(150);
            }
            ReturnPool();
        }
        else if (owner == BulletOwner.Player && other.CompareTag("EnemyBody"))
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(baseDamage);
                scoreManager?.AddScore(20);
            }
            ReturnPool();
        }
        else if (owner == BulletOwner.Enemy && other.CompareTag("PlayerBody"))
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(baseDamage);
            }
            ReturnPool();
        }
        else if (owner == BulletOwner.Enemy && other.CompareTag("PlayerHead"))
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(baseDamage * 3);
            }
            ReturnPool();
        }
        else if (other.CompareTag("Wall"))
        {
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
