using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("보유 무기 목록 (프리팹)")]
    public List<WeaponBase> weaponPrefabs = new List<WeaponBase>();

    [Header("필수 참조")]
    public Transform weaponHolder;     // 무기 모델이 배치될 부모 (WeaponRoot)
    public Transform playerFirePoint;  // 총구 발사 위치 (Muzzle)
    public PlayerShooter playerShooter; // 사격 로직 스크립트 참조

    private List<WeaponBase> weaponInstances = new List<WeaponBase>();
    private int currentIndex = 0;
    public WeaponBase CurrentWeapon { get; private set; }

    private void Start()
    {
        // 1. 기존 인스턴스 생성 로직 유지
        foreach (WeaponBase prefab in weaponPrefabs)
        {
            if (prefab == null) continue;
            WeaponBase w = Instantiate(prefab, weaponHolder);
            w.firePoint = playerFirePoint;
            w.gameObject.SetActive(false);
            weaponInstances.Add(w);
        }

        // 2. 첫 번째 무기 활성화 및 PlayerShooter 데이터 동기화
        if (weaponInstances.Count > 0)
        {
            currentIndex = 0;
            ActivateWeapon(currentIndex);
        }
    }

    private void Update()
    {
        // 입력 제한 확인 (기존 PlayerShooter 로직 준수)
        if (InputLockManager.Blocked) return;

        HandleScrollInput();
        HandleAlphaNumericInput();
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f) return;

        int nextIndex = scroll > 0
            ? (currentIndex + 1) % weaponInstances.Count
            : (currentIndex - 1 + weaponInstances.Count) % weaponInstances.Count;

        ActivateWeapon(nextIndex);
    }

    private void HandleAlphaNumericInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ActivateWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ActivateWeapon(1);
    }

    public void ActivateWeapon(int index)
    {
        if (index < 0 || index >= weaponInstances.Count) return;

        // 현재 무기와 같더라도 데이터 동기화를 위해 체크 생략 가능하지만, 효율을 위해 인덱스 저장
        WeaponBase targetWeapon = weaponInstances[index];
        if (targetWeapon == null || targetWeapon.weaponData == null) return;

        // 해금 여부 체크
        if (!targetWeapon.weaponData.isUnlocked)
        {
            Debug.Log($"{targetWeapon.weaponData.weaponName}은(는) 잠겨 있습니다.");
            return;
        }

        // 모든 무기 오브젝트 껐다 켜기
        for (int i = 0; i < weaponInstances.Count; i++)
        {
            weaponInstances[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
        CurrentWeapon = weaponInstances[currentIndex];

        // [핵심] PlayerShooter에 변경된 무기 데이터 전달 (리팩토링 포인트)
        if (playerShooter != null)
        {
            playerShooter.SetWeapon(CurrentWeapon.weaponData);
        }

        Debug.Log($"무기 교체 완료: {CurrentWeapon.weaponData.weaponName}");
    }
}
