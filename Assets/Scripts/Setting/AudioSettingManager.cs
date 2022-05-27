using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace com.louis.shootball
{
    public class AudioSettingManager : MonoBehaviour
    {
        [SerializeField]
        private AudioMixerGroup musicGroup;
        [SerializeField]
        private AudioMixerGroup sfxGroup;
        public Slider SfxSlider;
        public Slider MusicSlider;

        public void Awake()
        {
            float sfx = PlayerPrefs.GetFloat("SfxVolume" , 1f);
            float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
            if (SfxSlider != null) {
                SfxSlider.value = sfx;
            }
            if (MusicSlider != null) {
                MusicSlider.value = music;
            }

        }

        public void OnMusicSliderValueChange(float value) 
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            float volume = Mathf.Log10(value) * 20;
            musicGroup.audioMixer.SetFloat("MusicVolume", volume);
        }

        public void OnSfxSliderValueChange(float value)
        {
            PlayerPrefs.SetFloat("SfxVolume", value);
            float volume = Mathf.Log10(value) * 20;
            sfxGroup.audioMixer.SetFloat("SfxVolume", volume);
        }

    }
}