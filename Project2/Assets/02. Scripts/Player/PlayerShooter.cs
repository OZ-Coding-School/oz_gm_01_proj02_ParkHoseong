using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : MonoBehaviour
{
    [Header("총알 세팅")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 40f;
    [SerializeField] private Transform yawRoot;
    [SerializeField] private Transform pitchRoot;
    [SerializeField] private Transform gunMuzzle;
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("사격 / 줌 설정")]
    [SerializeField] private float fireRate = 0.1f;     // 연사 간격
    [SerializeField] private float zoomFOV = 20f;       // 스코프 배율 (3배 줌)
    [SerializeField] private float normalFOV = 60f;     // 기본 시야
    [SerializeField] private float zoomSpeed = 10f;     // 줌 전환 부드러움
    [SerializeField] private float zoomSensitivityMultiplier = 0.5f; // 줌 중 감도 감소

    [Header("총기 정렬")]
    [SerializeField] private Transform gunRoot;  // 총 루트
    [SerializeField] private Vector3 normalPos = new Vector3(0.4f, -0.3f, 0.5f); // 평소 위치
    [SerializeField] private Vector3 zoomPos = new Vector3(0f, -0.1f, 0.8f);    // 줌 시 중앙 위치
    [SerializeField] private float alignSpeed = 10f; // 이동 속도

    [Header("UI")]
    [SerializeField] private Image crosshair;     // 조준점 이미지
    [SerializeField] private Image scopeOverlay;  // 스코프 오버레이 이미지 (줌 시 표시)

    [Header("탄약 세팅")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int totalClips = 3;

    [Header("게임 관리")]
    [SerializeField] public ScoreManager scoreManager;

    [Header("Bullet Pool")]
    [SerializeField] private BulletManager bulletManager;

    public bool isInventoryOpen = false;

    private float pitch;
    private float nextFireTime;
    private bool isZooming;
    private bool isAutoFire = true; // true: 연사 / false: 단발
    private float originalSensitivity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentAmmo = maxAmmo;
        originalSensitivity = mouseSensitivity;

        if (cam != null)
            cam.fieldOfView = normalFOV;

        if (crosshair != null)
            crosshair.enabled = false;

        if (scopeOverlay != null)
            scopeOverlay.enabled = false;
    }

    void Update()
    {
        if (InputLockManager.Blocked)
            return;

        HandleLook();
        HandleFire();
        HandleReload();
        HandleZoom();
        HandleFireModeSwitch();
        HandleGunAlignment(); // 총기 정렬 처리
    }

    // 마우스 회전
    void HandleLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (yawRoot) yawRoot.Rotate(Vector3.up, mx, Space.World);
        pitch = Mathf.Clamp(pitch - my, -85f, 85f);
        if (pitchRoot) pitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // 발사 처리 (연사/단발)
    void HandleFire()
    {
        if (isAutoFire)
        {
            // 연사 모드: 좌클릭 유지
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
        else
        {
            // 단발 모드: 좌클릭 눌렀을 때만
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
    }

    // 재장전
    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Reload();
    }

    // 연사/단발 전환 (B키)
    void HandleFireModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isAutoFire = !isAutoFire;
            string mode = isAutoFire ? "연사" : "단발";
            Debug.Log($"사격 모드 전환: {mode}");
        }
    }

    void HandleZoom()
    {
        if (cam == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            isZooming = true;
            mouseSensitivity = originalSensitivity * zoomSensitivityMultiplier;

            // 줌 시 스코프 오버레이만 표시
            if (scopeOverlay != null) scopeOverlay.enabled = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isZooming = false;
            mouseSensitivity = originalSensitivity;

            // 줌 해제 시 오버레이 비활성화
            if (scopeOverlay != null) scopeOverlay.enabled = false;
        }

        float targetFOV = isZooming ? zoomFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    // 줌 중 총기 위치 중앙 정렬
    void HandleGunAlignment()
    {
        if (gunRoot == null) return;

        Vector3 targetPos = isZooming ? zoomPos : normalPos;
        gunRoot.localPosition = Vector3.Lerp(gunRoot.localPosition, targetPos, Time.deltaTime * alignSpeed);
    }

    void Shoot()
    {
        if (cam == null || gunMuzzle == null)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log("탄약 부족! 재장전 필요");
            return;
        }

        Vector3 dir = cam.transform.forward;
        if (bulletManager == null)
        {
            Debug.Log("BulletManager가 연결되지 않았습니다");
            return;
        }

        GameObject go = bulletManager.GetBulletPrefab();
        if (go == null)
            return;

        go.transform.position = gunMuzzle.position;
        go.transform.rotation = Quaternion.LookRotation(dir);
        go.SetActive(true);

        if (go.TryGetComponent(out Bullet b))
        {
            b.scoreManager = scoreManager;
            b.Shot(dir, bulletSpeed);
        }

        currentAmmo--;
        scoreManager?.ConsumeAmmo(currentAmmo, totalClips);
        Debug.Log($"발사! 남은 탄약: {currentAmmo}/{maxAmmo}, 예비 탄창: {totalClips}");
    }

    void Reload()
    {
        if (totalClips > 0 && currentAmmo < maxAmmo)
        {
            totalClips--;
            currentAmmo = maxAmmo;
            scoreManager?.ConsumeAmmo(currentAmmo, totalClips);
            Debug.Log($"재장전 완료! 현재 탄약: {currentAmmo}/{maxAmmo}, 예비 탄창: {totalClips}");
        }
        else
        {
            Debug.Log("재장전 불가 (예비 탄창 없음)");
        }
    }

    public void AddAmmo(int clips)
    {
        totalClips += clips;
        scoreManager?.ConsumeAmmo(currentAmmo, totalClips);
        Debug.Log($"예비 탄창 추가! 현재 예비 탄창: {totalClips}");
    }
}