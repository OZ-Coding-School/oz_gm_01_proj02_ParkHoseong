using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [Header("데이터 참조")]
    public WeaponData weaponData;

    [Header("총구 위치")]
    public Transform firePoint;

    public int currentAmmo;
    public int currentTotalClips;

    private void Awake()
    {
        //처음 시작할 때만 SO의 기본값으로 초기화
        if (weaponData != null)
        {
            currentAmmo = weaponData.maxAmmo;
            currentTotalClips = weaponData.maxMag;
        }
    }
}
