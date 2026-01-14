using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "FPS/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("무기 정보")]
    public string weaponName = "M4";       // DataManager에서 식별자로 사용
    public bool isUnlocked = true;

    [Header("전투 설정")]
    public float damage = 10.0f;
    [Tooltip("연사 간격(초)")]
    public float fireRate = 0.1f;
    [Tooltip("재장전 시간(초)")]
    public float reloadTime = 1.0f;
    [Tooltip("한 발씩 장전되는 무기인가?")]
    public bool isSingleLoad = false;

    [Header("탄약 설정")]
    public int maxAmmo = 30;
    public int maxMag = 3;

    [Header("반동 설정")]
    public float recoilX = 2.0f; // 위로 튀는 힘
    public float recoilY = 0.5f; // 좌우 흔들림
    public float snappiness = 10f;
    public float returnSpeed = 5f;

    [Header("사운드 설정 (추가됨)")]
    public AudioClip fireSound;    //사격 소리
    public AudioClip reloadSound;  //재장전 소리
}
