using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap {
    public class CalibrationHandle : MonoBehaviour
    {
        [SerializeField] MotionCaptureController[] imuCalibs;
        [SerializeField] LeapCalibration leapCalib;
        Button bothButton, leapButton, imuButton;
        Text count;


        void Start()
        {
            //UISetting.SetButton(ref bothButton, this.transform, "Both", OnBtn_Both);
            UISetting.SetButton(ref leapButton, this.transform, "Leap Calibration", () => StartCoroutine(OnBtn_Leap()));
            UISetting.SetButton(ref imuButton, this.transform, "IMU Sensor", () => StartCoroutine(OnBtn_IMU(3)));
            count = GameObject.Find("Calibration Count Down").GetComponent<Text>();
            count.gameObject.SetActive(false);
        }

        void OnBtn_Both()
        {
            Debug.Log("Both Calibrate");
        }

        IEnumerator OnBtn_Leap()
        {
            UISetting.SetButtonColor(leapButton, Color.cyan);
            yield return leapCalib.Calibration(count, 3);
            UISetting.SetButtonColor(leapButton, Color.white);
        }

        IEnumerator OnBtn_IMU(int second)
        {
            UISetting.SetButtonColor(imuButton, Color.cyan);
            count.gameObject.SetActive(true);
            count.text = second.ToString();

            /* カウントダウン */
            var wait = new WaitForSeconds(1.0f);
            for (int i = second - 1; i >= 0; i--)
            {
                /* 前の数字を1秒間表示してから数字を更新する */
                yield return wait;
                count.text = i.ToString();
            }

            count.text = "Recording";
            foreach (var imuCalib in imuCalibs) imuCalib.Calib();

            bool calibrating;
            do
            {
                calibrating = false;
                foreach (var imuCalib in imuCalibs)
                {
                    calibrating |= imuCalib.calibrationLevel == MotionCaptureController.CalibrationLevel.Recording;
                }
                yield return null;
            } while (calibrating);

            count.gameObject.SetActive(false);            
            do
            {
                calibrating = false;
                foreach (var imuCalib in imuCalibs)
                {
                    calibrating |= imuCalib.calibrationLevel == MotionCaptureController.CalibrationLevel.Calculating;
                }
                yield return null;
            } while (calibrating);
            foreach (var imuCalib in imuCalibs) imuCalib.Ready();
            UISetting.SetButtonColor(imuButton, Color.white);
        }

        private void OnApplicationQuit()
        {
            foreach (var imuCalib in imuCalibs) imuCalib.End();
        }
    }
}
