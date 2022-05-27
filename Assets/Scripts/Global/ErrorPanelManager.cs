using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace com.louis.shootball
{
    public class ErrorPanelManager : MonoBehaviour
    {
        public static ErrorPanelManager Instance;
        public TMP_Text ErrorText;
        public GameObject ErrorPanel;

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

        public void OnErrorMesssage(string message) {
            ErrorText.text = message;
            ErrorPanel.SetActive(true);
        }
    }
}