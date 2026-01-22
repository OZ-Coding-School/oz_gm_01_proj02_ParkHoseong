using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("데이터 참조 (SO)")]
    //모든 무기 데이터를 여기에 넣어두면, 저장/로드 시 루프를 돌며 처리
    public List<WeaponData> allWeaponData = new();

    [Header("스테이지 정보")]
    //스테이지별 클리어 여부를 인덱스로 관리 (예: 0=Tutorial, 1=Stage01...)
    public int[] stageClearMask = new int[5];
    public int stageCount => stageClearMask.Length;

    public static int TotalScore = 0;
    private const string BestScoreKey = "BestScore";

    public enum MissionMode
    {
        Scout = 0,
        Annihilation = 1,
        Infinite = 2
    }

    public int selectedMode = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //게임 시작 시 기존 저장된 해금/클리어 정보를 불러옵니다.
        Load();
    }

    public bool IsCleared(int stageIndex, int mode)
    {
        if (stageIndex < 0 || stageIndex >= stageCount) return false;
        if (mode < 0 || mode > 2) return false;

        int bit = 1 << mode;
        return (stageClearMask[stageIndex] & bit) != 0;
    }

    public bool IsUnlocked(int stageIndex, int mode)
    {
        if (mode < 0 || mode > 2) return false;

        //Tutorial(stage 0): only mode 0
        if (stageIndex == 0) return mode == 0;

        //Mode 0: unlocked by previous stage mode 1 (except stage 1 uses tutorial mode 0)
        if (mode == 0)
        {
            if (stageIndex == 1) return IsCleared(0, 0);
            return IsCleared(stageIndex - 1, 1);
        }

        //Mode 1: unlocked by current stage mode 0
        if (mode == 1)
            return IsCleared(stageIndex, 0);

        //Mode 2: unlocked by current stage mode 1
        if (mode == 2)
            return IsCleared(stageIndex, 1);

        return false;
    }

    //특정 무기를 해금할 때 호출
    public void UnlockWeapon(string weaponName)
    {
        WeaponData weapon = allWeaponData.Find(w => w.weaponName == weaponName);
        if (weapon != null)
        {
            weapon.isUnlocked = true;
            Save();
        }
    }

    public void ClearMission(int stageIndex, int mode)
    {
        if (stageIndex < 0 || stageIndex >= stageCount) return;
        if (mode < 0 || mode > 2) return;

        int bit = 1 << mode;
        stageClearMask[stageIndex] |= bit;
        Debug.Log($"[DataManager] Stage {stageIndex} - Mode {mode} Cleared!");
        Save();
    }

    public void Save()
    {
        //무기 해금 정보 저장
        foreach (var weapon in allWeaponData)
        {
            if (weapon == null) continue;
            //각 무기의 고유 이름을 키값으로 사용하여 해금 여부(0 또는 1) 저장
            PlayerPrefs.SetInt($"{weapon.weaponName}_Unlocked", weapon.isUnlocked ? 1 : 0);
        }

        //스테이지 클리어 정보 저장
        for (int i = 0; i < stageCount; i++)
        {
            PlayerPrefs.SetInt($"Stage_{i}_ClearMask", stageClearMask[i]);
        }

        PlayerPrefs.Save();
        Debug.Log("[DataManager] 모든 데이터 저장 완료");
    }

    public void Load()
    {
        //무기 해금 정보 로드
        foreach (var weapon in allWeaponData)
        {
            if (weapon == null) continue;
            //기본 무기 등 특정 조건이 필요하면 여기서 기본값(0 또는 1)을 설정 가능
            int defaultValue = weapon.isUnlocked ? 1 : 0;
            weapon.isUnlocked = PlayerPrefs.GetInt($"{weapon.weaponName}_Unlocked", defaultValue) == 1;
        }

        //스테이지 클리어 정보 로드
        for (int i = 0; i < stageCount; i++)
        {
            stageClearMask[i] = PlayerPrefs.GetInt($"Stage_{i}_ClearMask", 0);
        }

        Debug.Log("[DataManager] 데이터 로드 완료");
    }

    public static void AddScore(int amount)
    {
        TotalScore += amount;

        //실시간으로 최고 기록 갱신 여부 확인
        int currentBest = GetBestScore();
        if (TotalScore > currentBest)
        {
            PlayerPrefs.SetInt(BestScoreKey, TotalScore);
            PlayerPrefs.Save();
        }
    }

    //최고 기록 가져오기
    public static int GetBestScore()
    {
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    //새로운 게임/스테이지 시작 시 현재 점수만 리셋
    public static void ResetCurrentScore()
    {
        TotalScore = 0;
    }

    public bool HasAnyProgress()
    {
        if (stageClearMask == null) return false;

        for (int i = 0; i < stageClearMask.Length; i++)
        {
            if (stageClearMask[i] != 0) return true;
        }
        return false;
    }
}
