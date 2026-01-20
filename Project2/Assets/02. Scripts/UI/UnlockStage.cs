using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UnlockStage : MonoBehaviour
{
    [Header("스테이지 설정")]
    [SerializeField] private int stageIndex;

    [SerializeField] private int mode = 0;

    //[Header("UI 설정")]
    //public Sprite lockSprite;   //잠긴 상태 이미지
    //public Sprite unlockSprite; //해금된 상태 이미지

    //private Image targetImage;
    //private Button stageButton;
    //private bool isUnlocked = false;

    //private void Awake()
    //{
    //    targetImage = GetComponent<Image>();
    //    stageButton = GetComponent<Button>();
    //}

    //private void Start()
    //{
    //    UpdateUnlockStatus();
    //}

    //public void UpdateUnlockStatus()
    //{
    //    if (stageIndex == 0)
    //    {
    //        isUnlocked = true;
    //    }
    //    else
    //    {
    //        if (DataManager.Instance != null && stageIndex <= DataManager.Instance.stageCleared.Length)
    //        {
    //            isUnlocked = DataManager.Instance.stageCleared[stageIndex - 1];
    //        }
    //    }
    //    ApplyUI();
    //}

    //private void ApplyUI()
    //{
    //    if (targetImage != null)
    //    {
    //        targetImage.sprite = isUnlocked ? unlockSprite : lockSprite;
    //    }

    //    if (stageButton != null)
    //    {
    //        stageButton.interactable = isUnlocked;
    //    }
    //}
    /// <summary>
    /// 위는 해금 이미지 생기면 쓰고, 그 전까진 일단 켜고 끄는 식으로만
    /// </summary>
    public void UpdateUnlockStatus()
    {
        bool isUnlocked = false;

        if (DataManager.Instance != null)
        {
            if (stageIndex >= 0 && stageIndex < DataManager.Instance.stageCount)
            {
                int m = mode;
                if (m < 0) m = 0;
                if (m > 2) m = 2;

                isUnlocked = DataManager.Instance.IsUnlocked(stageIndex, m);
            }
        }

        this.gameObject.SetActive(isUnlocked);
    }

    private void Start()
    {
        UpdateUnlockStatus();
    }
}
