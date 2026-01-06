using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize=60;

    private GameObject[] pool;

    private void Awake()
    {
        CreatePool();
    }

    void CreatePool()
    { 
        pool=new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(bulletPrefab, transform);
            pool[i].SetActive(false);
        }
    }

    public GameObject GetBulletPrefab() 
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].activeSelf)
                return pool[i];
        }
        return null;
    }
}
