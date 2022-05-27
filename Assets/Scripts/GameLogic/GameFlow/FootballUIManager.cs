using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace com.louis.shootball
{
    public class FootballUIManager : MonoBehaviour
    {
        public static FootballUIManager Instance;

        public TMP_Text BlueScoreText;
        public TMP_Text RedScoreText;
        public TMP_Text TimeText;
        public GameObject Menu;
        public GameObject DrawBanner;
        public GameObject RedBanner;
        public GameObject BlueBanner;
        public bool toggled = false;
        public bool shouldLockMouse = false;
        public Slider volumeSlider;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            #if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)
            #else
            if (shouldLockMouse) { 
                SetCursorState(true);
            }
#endif
            volumeSlider.value = PlayerPrefs.GetFloat("Volume");

        }

        public void Update()
        {
            BlueScoreText.text = FootballGameManager.Instance.blueScore.ToString("0");
            RedScoreText.text = FootballGameManager.Instance.redScore.ToString("0");
            float totalSeconds = FootballGameManager.Instance.GameLength - FootballGameManager.Instance.timer;
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
            TimeText.text = time.ToString("mm':'ss");
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public void OnToggleMenu(InputValue value)
        {
            toggled = !toggled;
            Menu.SetActive(toggled);

            if (shouldLockMouse)
            {
                SetCursorState(!toggled);
            }
        }

        public void OnCloseMenu()
        {
            toggled = false;
            Menu.SetActive(false);

            if (shouldLockMouse)
            {
                SetCursorState(true);
            }
        }

        public void ShowBanner(Team WinningTeam) {
            if (WinningTeam == Team.RED)
            {
                RedBanner.SetActive(true);
            }
            else if (WinningTeam == Team.BLUE)
            {
                BlueBanner.SetActive(true);
            }
            else {
                DrawBanner.SetActive(true);
            }
        }

        public void OnAudioSliderChange(Single value) {
            PlayerPrefs.SetFloat("Volume", value);
        }

    }

}