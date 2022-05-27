using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.louis.shootball
{
    public class ScreenCaptureManager : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                ScreenCapture.CaptureScreenshot(Application.dataPath + "/ScreenCapture/" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
            }
        }
    }
}