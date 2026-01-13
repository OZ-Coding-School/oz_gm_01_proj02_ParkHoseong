using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "FPS/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("반동 설정")]
    public float recoilX = 2.0f; // 위로 튀는 힘
    public float recoilY = 0.5f; // 좌우 흔들림
    public float snappiness = 10f;
    public float returnSpeed = 5f;
}
