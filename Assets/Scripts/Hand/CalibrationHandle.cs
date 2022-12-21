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
            UISetting.SetButton(ref bothButton, this.transform, "Both", OnBtn_Both);
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

            foreach (var imuCalib in imuCalibs) imuCalib.Calib();
            bool calibrating = false;
            do
            {
                foreach (var imuCalib in imuCalibs)
                {
                    calibrating |= imuCalib.runMode == MotionCaptureController.RunMode.Calibrating;
                }
                yield return null;
            } while (calibrating);

            /* 0.5秒間「0」を表示してからカウント表示を消す */
            yield return new WaitForSeconds(0.5f);
            count.gameObject.SetActive(false);
            UISetting.SetButtonColor(imuButton, Color.white);
        }

        private void OnApplicationQuit()
        {
            foreach (var imuCalib in imuCalibs) imuCalib.End();
        }
    }
}
