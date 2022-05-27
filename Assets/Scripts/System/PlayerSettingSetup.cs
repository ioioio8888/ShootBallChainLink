using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Audio;

namespace com.louis.shootball
{
    public class PlayerSettingSetup : MonoBehaviour
    {
        [SerializeField]
        private AudioMixerGroup musicGroup;
        [SerializeField]
        private AudioMixerGroup sfxGroup;
        private void Awake()
        {
            SetupPhoton();
            SetUpVolume();
            SetupGraphics();
        }

        void SetupPhoton()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.EnableCloseConnection = true;
            PhotonNetwork.KeepAliveInBackground = 0f;
        }

        void SetupGraphics() {
            int quality = PlayerPrefs.GetInt("Quality", 5);
            //QualitySettings.SetQualityLevel(quality, true);
        }

        void SetUpVolume() {
            float sfx = PlayerPrefs.GetFloat("SfxVolume", 1f);
            float music = PlayerPrefs.GetFloat("MusicVolume", 1f); 
            float sfxvolume = Mathf.Log10(sfx) * 20;
            sfxGroup.audioMixer.SetFloat("SfxVolume", sfxvolume);
            float musicvolume = Mathf.Log10(music) * 20;
            musicGroup.audioMixer.SetFloat("MusicVolume", musicvolume);
        }
    }
}