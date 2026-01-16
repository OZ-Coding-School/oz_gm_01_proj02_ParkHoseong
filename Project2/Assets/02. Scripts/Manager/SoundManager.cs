using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmPlayer;
    public AudioClip defaultBgm;
    [Range(0, 1)] public float bgmVolume = 0.5f;

    [Header("SFX")]
    [SerializeField] private int channels = 10; // 중첩 재생 채널 수
    [Range(0, 1)] public float sfxVolume = 0.5f;
    private AudioSource[] sfxPlayers;
    private int channelIndex;
    public float GetSfxVolume() => sfxVolume;
    public float GetBgmVolume() => bgmVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        // BGM 설정
        if (bgmPlayer == null)
        {
            GameObject bgmObject = new GameObject("BgmPlayer");
            bgmObject.transform.parent = transform;
            bgmPlayer = bgmObject.AddComponent<AudioSource>();
        }
        bgmPlayer.loop = true;
        bgmPlayer.playOnAwake = false;
        bgmPlayer.volume = bgmVolume;

        // SFX 채널 생성
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].bypassListenerEffects = true;
            sfxPlayers[i].volume = sfxVolume;
        }
        if (bgmPlayer != null && defaultBgm != null)
        {
            bgmPlayer.clip = defaultBgm;
            bgmPlayer.loop = true;
            bgmPlayer.playOnAwake = true;
            bgmPlayer.Play();
            Debug.Log($"BGM 재생 시작: {defaultBgm.name}");
        }
    }

    // WeaponData의 AudioClip을 직접 재생하는 핵심 함수
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        // 라운드 로빈 방식으로 빈 채널 찾아 재생
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying) continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = clip;
            sfxPlayers[loopIndex].Play();
            return;
        }

        // 모든 채널이 사용 중일 경우 가장 오래된 채널 강제 할당
        channelIndex = (channelIndex + 1) % sfxPlayers.Length;
        sfxPlayers[channelIndex].clip = clip;
        sfxPlayers[channelIndex].Play();
    }
    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        if (bgmPlayer != null)
        {
            bgmPlayer.volume = bgmVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        foreach (var p in sfxPlayers) p.volume = sfxVolume;
    }
}
