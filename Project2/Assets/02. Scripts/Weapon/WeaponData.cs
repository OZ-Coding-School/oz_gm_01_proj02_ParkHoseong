using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "FPS/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("무기 정보")]
    public string weaponName = "M4";       //DataManager에서 식별자로 사용
    public bool isUnlocked = true;
    public bool isAuto = true;
    public bool isSingleLoad = false;

    [Header("전투 설정")]
    public float damage = 10.0f;
    public float fireRate = 0.1f;
    public float reloadTime = 1.0f;
    public float bulletSpeed = 40f;

    [Header("탄약 설정")]
    public int maxAmmo = 30;
    public int maxMag = 3;

    [Header("줌 설정")]
    public float zoomMagnification = 1.0f;

    [Header("반동 설정")]
    public float recoilX = 2.0f; //위로 튀는 힘
    public float recoilY = 0.5f; //좌우 흔들림
    public float snappiness = 10f;
    public float returnSpeed = 5f;

    [Header("UI 설정")]
    public Sprite crosshairSprite;      //이 무기가 사용할 지향사격 UI (없으면 null)
    public bool useCrosshair = true;    //지향사격 시 UI를 보여줄지 여부

    [Header("사운드 설정 (추가됨)")]
    public AudioClip fireSound;    //사격 소리
    public AudioClip reloadSound;  //재장전 소리
}
