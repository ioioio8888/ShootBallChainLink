using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace com.louis.shootball
{
    public class GraphicSettingManager : MonoBehaviour
    {
        int qualityLevel = 0;
        public Toggle LowToggle;
        public Toggle MidToggle;
        public Toggle HighToggle;

        // Start is called before the first frame update
        void Start()
        {
            SetQualityUI();
        }

        void SetQualityUI() {
            if (LowToggle == null || MidToggle == null || HighToggle == null) {
                //Toggle doesnt exist
                return;
            }
            qualityLevel = QualitySettings.GetQualityLevel();
            switch (qualityLevel)
            {
                case 1:
                    LowToggle.SetIsOnWithoutNotify(true);
                    LowToggle.interactable = false;
                    MidToggle.SetIsOnWithoutNotify(false);
                    MidToggle.interactable = true;
                    HighToggle.SetIsOnWithoutNotify(false);
                    HighToggle.interactable = true;
                    break;
                case 3:
                    LowToggle.SetIsOnWithoutNotify(false);
                    LowToggle.interactable = true;
                    MidToggle.SetIsOnWithoutNotify(true);
                    MidToggle.interactable = false;
                    HighToggle.SetIsOnWithoutNotify(false);
                    HighToggle.interactable = true;
                    break;
                case 5:
                    LowToggle.SetIsOnWithoutNotify(false);
                    LowToggle.interactable = true;
                    MidToggle.SetIsOnWithoutNotify(false);
                    MidToggle.interactable = true;
                    HighToggle.SetIsOnWithoutNotify(true);
                    HighToggle.interactable = false;
                    break;
            }
        }

        public void OnLowQualitySelected() {
            PlayerPrefs.SetInt("Quality", 1);
            QualitySettings.SetQualityLevel(1, false);
            SetQualityUI();
        }

        public void OnMidQualitySelected()
        {
            PlayerPrefs.SetInt("Quality", 3);
            QualitySettings.SetQualityLevel(3, false);
            SetQualityUI();
        }
        public void OnHighQualitySelected()
        {
            PlayerPrefs.SetInt("Quality", 5);
            QualitySettings.SetQualityLevel(5, false);
            SetQualityUI();
        }

    }
}