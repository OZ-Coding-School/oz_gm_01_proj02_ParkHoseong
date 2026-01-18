using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : MonoBehaviour
{
    [Header("총알 세팅")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform yawRoot;
    [SerializeField] private Transform pitchRoot;
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("줌 설정")]
    [SerializeField] private float normalFOV = 60f;     //기본 시야
    [SerializeField] private float zoomSpeed = 10f;     //줌 전환 부드러움
    [SerializeField] private float zoomSensitivityMultiplier = 0.5f; //줌 중 감도 감소

    [Header("총기 정렬")]
    [SerializeField] private Transform gunRoot;  //총 루트
    [SerializeField] private Vector3 normalPos = new Vector3(0.4f, -0.3f, 0.5f); //평소 위치
    [SerializeField] private Vector3 zoomPos = new Vector3(0f, -0.1f, 0.8f);    //줌 시 중앙 위치
    [SerializeField] private float alignSpeed = 10f; //이동 속도

    [Header("UI")]
    [SerializeField] private Image crosshair;     //조준점 이미지

    [Header("게임 관리")]
    [SerializeField] public ScoreManager scoreManager;

    [Header("Bullet Pool")]
    [SerializeField] private BulletManager bulletManager;

    [Header("Animation (Direct Assignment)")]
    [SerializeField] private Animator animator;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    public bool isInventoryOpen = false;

    private float pitch;
    private float nextFireTime;
    private bool isZooming;
    private bool isAutoFire = true; //true: 연사 / false: 단발
    private float originalSensitivity;

    private WeaponBase activeWeaponObject;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (activeWeaponObject != null && activeWeaponObject.weaponData != null)
        {
            scoreManager?.ConsumeAmmo(activeWeaponObject.currentAmmo, activeWeaponObject.currentTotalClips, activeWeaponObject.weaponData);
        }

        originalSensitivity = mouseSensitivity;

        if (cam != null)
            cam.fieldOfView = normalFOV;

        if (crosshair != null)
            crosshair.enabled = false;
    }

    void Update()
    {
        if (InputLockManager.Blocked)
            return;

        if (activeWeaponObject == null || activeWeaponObject.weaponData == null) 
            return;

        HandleLook();
        HandleFire();
        HandleReload();
        HandleZoom();
        HandleFireModeSwitch();
        HandleGunAlignment(); //총기 정렬 처리
    }

    // 마우스 회전
    void HandleLook()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, activeWeaponObject.weaponData.returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, activeWeaponObject.weaponData.snappiness * Time.fixedDeltaTime);

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (yawRoot) yawRoot.Rotate(Vector3.up, mx + (currentRotation.y*0.1f), Space.World);
        pitch = Mathf.Clamp(pitch - my - (currentRotation.x*0.1f), -85f, 85f);
        if (pitchRoot) pitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // 발사 처리 (연사/단발)
    void HandleFire()
    {
        if (activeWeaponObject.weaponData.isAuto && isAutoFire)
        {
            // 연사 모드: 좌클릭 유지
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + activeWeaponObject.weaponData.fireRate;
                Shoot();
            }
        }
        else
        {
            // 단발 모드: 좌클릭 눌렀을 때만
            if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + activeWeaponObject.weaponData.fireRate;
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
        if (activeWeaponObject.weaponData.isAuto && Input.GetKeyDown(KeyCode.B))
        {
            isAutoFire = !isAutoFire;
            string mode = isAutoFire ? "연사" : "단발";     //UI에 표기할 예정임
            Debug.Log($"사격 모드 전환: {mode}");
        }
    }

    void HandleZoom()
    {
        if (cam == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            isZooming = true;
            if (animator != null) animator.SetBool("IsAiming", true);
            mouseSensitivity = originalSensitivity * zoomSensitivityMultiplier;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isZooming = false;
            mouseSensitivity = originalSensitivity;
            if (animator != null) animator.SetBool("IsAiming", false);
        }

        float targetFOV = isZooming ? (normalFOV / activeWeaponObject.weaponData.zoomMagnification) : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        if (crosshair != null)
        {
            bool shouldShow = !isZooming && activeWeaponObject.weaponData.useCrosshair;
            crosshair.enabled = shouldShow;
        }
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
        if (cam == null || activeWeaponObject.firePoint == null)
            return;

        if (activeWeaponObject.currentAmmo <= 0)
        {
            Debug.Log("탄약 부족! 재장전 필요");
            return;
        }

        if (animator != null) animator.SetTrigger("Attack");

        targetRotation += new Vector3(-activeWeaponObject.weaponData.recoilX,
            Random.Range(-activeWeaponObject.weaponData.recoilY, activeWeaponObject.weaponData.recoilY), 0);

        Vector3 dir = cam.transform.forward;
        if (bulletManager == null)
        {
            Debug.Log("BulletManager가 연결되지 않았습니다");
            return;
        }

        GameObject go = bulletManager.GetBulletPrefab();
        if (go == null)
            return;

        go.transform.position = activeWeaponObject.firePoint.position;
        go.transform.rotation = Quaternion.LookRotation(dir);
        go.SetActive(true);

        if (go.TryGetComponent(out Bullet b))
        {
            b.scoreManager = scoreManager;
            b.baseDamage = (int)activeWeaponObject.weaponData.damage;
            b.Shot(dir, activeWeaponObject.weaponData.bulletSpeed);
        }

        activeWeaponObject.currentAmmo--;
        scoreManager?.ConsumeAmmo(activeWeaponObject.currentAmmo, activeWeaponObject.currentTotalClips, activeWeaponObject.weaponData);
        Debug.Log($"발사! 남은 탄약: {activeWeaponObject.currentAmmo}/{activeWeaponObject.weaponData.maxAmmo}, 예비 탄창: {activeWeaponObject.currentTotalClips}");

        if (activeWeaponObject.weaponData.fireSound != null)
        {
            SoundManager.Instance.PlaySfx(activeWeaponObject.weaponData.fireSound);
        }
    }

    void Reload()
    {
        if (activeWeaponObject.currentTotalClips > 0 && activeWeaponObject.currentAmmo < activeWeaponObject.weaponData.maxAmmo)
        {
            if (animator != null) animator.SetTrigger("Reload");

            activeWeaponObject.currentTotalClips--;
            activeWeaponObject.currentAmmo = activeWeaponObject.weaponData.maxAmmo;

            scoreManager?.ConsumeAmmo(activeWeaponObject.currentAmmo, activeWeaponObject.currentTotalClips, activeWeaponObject.weaponData);

            if (activeWeaponObject.weaponData.reloadSound != null)
                SoundManager.Instance.PlaySfx(activeWeaponObject.weaponData.reloadSound);
            Debug.Log($"재장전 완료! 현재 탄약: {activeWeaponObject.currentAmmo}/{activeWeaponObject.weaponData.maxAmmo}, 예비 탄창: {activeWeaponObject.currentTotalClips}");
        }
        else
        {
            Debug.Log("재장전 불가 (예비 탄창 없음)");
        }
    }

    public void AddAmmo(int clips)
    {
        activeWeaponObject.currentTotalClips += clips;
        scoreManager?.ConsumeAmmo(activeWeaponObject.currentAmmo, activeWeaponObject.currentTotalClips, activeWeaponObject.weaponData);
        Debug.Log($"예비 탄창 추가! 현재 예비 탄창: {activeWeaponObject.currentTotalClips}");
    }

    public void SetWeapon(WeaponBase newWeapon)
    {
        if (newWeapon == null) return;

        activeWeaponObject = newWeapon;

        isZooming = false;

        if (!activeWeaponObject.weaponData.isAuto) isAutoFire = false;

        if (crosshair != null)
        {
            crosshair.sprite = activeWeaponObject.weaponData.crosshairSprite;
            crosshair.enabled = activeWeaponObject.weaponData.useCrosshair;
        }

        scoreManager?.ConsumeAmmo(activeWeaponObject.currentAmmo, activeWeaponObject.currentTotalClips, activeWeaponObject.weaponData);
    }
}