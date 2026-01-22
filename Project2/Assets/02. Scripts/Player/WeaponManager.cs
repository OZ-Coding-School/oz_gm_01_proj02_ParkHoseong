using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("필수 참조")]
    public Transform weaponHolder; 
    public PlayerShooter playerShooter;

    private List<WeaponBase> weaponInstances = new List<WeaponBase>();
    private int currentIndex = 0;

    private void Start()
    {
        foreach (Transform child in weaponHolder)
        {
            WeaponBase w = child.GetComponent<WeaponBase>();
            if (w != null)
            {
                if (w.weaponData.isUnlocked)
                {
                    weaponInstances.Add(w);
                }

                child.gameObject.SetActive(false); //초기화 시 모두 비활성화
            }
        }

        if (weaponInstances.Count > 0)
        {
            ActivateWeapon(0);
        }
    }

    private void Update()
    {
        if (InputLockManager.Blocked) return;

        HandleScrollInput();
        HandleNumericInput();
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

    private void HandleNumericInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ActivateWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ActivateWeapon(1);
    }

    public void ActivateWeapon(int index)
    {
        if (index < 0 || index >= weaponInstances.Count) return;

        // 모든 무기 오브젝트 상태 제어
        for (int i = 0; i < weaponInstances.Count; i++)
        {
            weaponInstances[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
        WeaponBase current = weaponInstances[currentIndex];

        Debug.Log($"<color=cyan>[WeaponManager]</color> 휠 조작 감지! 현재 인덱스: {index} | 무기명: {current.weaponData.weaponName}");

        if (playerShooter != null && current.weaponData != null)
        {
            playerShooter.SetWeapon(current);
        }
    }
}
