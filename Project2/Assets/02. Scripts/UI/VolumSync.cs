using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumSync : MonoBehaviour
{
    public enum VolumeType { BGM, SFX }
    public VolumeType volumeType;

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        if (SoundManager.Instance != null)
        {
            float currentVol = (volumeType == VolumeType.BGM)
                ? SoundManager.Instance.GetBgmVolume()
                : SoundManager.Instance.GetSfxVolume();
            slider.value = currentVol;
        }

        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        if (SoundManager.Instance == null) return;

        if (volumeType == VolumeType.BGM)
        {
            SoundManager.Instance.SetBgmVolume(value);
            Debug.Log($"BGM 볼륨 변경됨: {value}");
        }
        else
        {
            SoundManager.Instance.SetSfxVolume(value);
            Debug.Log($"SFX 볼륨 변경됨: {value}");
        }
    }
}
