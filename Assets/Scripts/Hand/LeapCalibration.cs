using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class LeapCalibration : MonoBehaviour
    {
        [SerializeField] GameObject left, right, midPoint, hands;

        [SerializeField] AverageMethod averageMode = AverageMethod.Slerp;

        [SerializeField, Range(0.1f, 200.0f)] float scaleForLeapMotion = 80.0f;

        public Vector3 adjustPos = Vector3.zero;

        Vector3 midPos = new Vector3(-2.31645107f, -32.073246f, 5.7862258f);
        Button calibrationButton;
        Text count;

        public enum AverageMethod {Lerp, Slerp}
        delegate Quaternion AverageQuaternion(Quaternion q1, Quaternion q2);

        readonly Dictionary<AverageMethod, AverageQuaternion> Avg
            = new Dictionary<AverageMethod, AverageQuaternion>() {
                { AverageMethod.Lerp,  (q1, q2) => Quaternion.Lerp(q1, q2, 0.5f) },
                { AverageMethod.Slerp, (q1, q2) => Quaternion.Slerp(q1, q2, 0.5f) }
            };


        void Start()
        {
            UISetting.SetButton(ref calibrationButton, "Calibration", () => StartCoroutine(CountDown(3)));
            count = GameObject.Find("Calibration Count Down").GetComponent<Text>();
            count.gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateMidPoint();
            left.GetComponent<LeapHandModel>().scl = scaleForLeapMotion;
            right.GetComponent<LeapHandModel>().scl = scaleForLeapMotion;
            hands.transform.localPosition = -midPos + adjustPos;
        }

        /// <summary>
        /// カウントダウンを行い、0秒になったらキャリブレーションが実行される。
        /// </summary>
        /// <param name="second">カウントダウンの秒数</param>
        /// <returns></returns>
        IEnumerator CountDown(int second)
        {
            /* 前処理として手のポーズをキャリブレーション用にする */
            left.GetComponent<HandSetting>().SetCalibrationPose();
            right.GetComponent<HandSetting>().SetCalibrationPose();

            /* カウントダウンの数字を表示 */
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

            /* カウントが0になったフレームで回転のキャリブレーション */
            CalibrateRotation();

            /* 回転が補正されて1フレーム待ってから位置を補正する */
            yield return null;
            CalibratePosition();

            /* 後処理として手のポーズをもとに戻す */
            left.GetComponent<HandSetting>().SetNormalPose();
            right.GetComponent<HandSetting>().SetNormalPose();

            /* 0.5秒間「0」を表示してからカウント表示を消す */
            yield return new WaitForSeconds(0.5f);
            count.gameObject.SetActive(false);
        } 

        /// <summary>
        /// 位置のキャリブレーション。
        /// </summary>
        void CalibratePosition()
        {
            midPos = 0.5f * (left.transform.localPosition + right.transform.localPosition);
            hands.transform.localPosition = adjustPos - midPos;
        }

        /// <summary>
        /// 回転のキャリブレーション。
        /// </summary>
        void CalibrateRotation()
        {
            Quaternion midRot = Avg[averageMode](left.transform.rotation, right.transform.rotation);
            this.transform.rotation = Quaternion.Inverse(midRot) * this.transform.rotation;
        }

        void UpdateMidPoint()
        {
            if (midPoint == null) return;
            midPoint.transform.position = 0.5f * (left.transform.position + right.transform.position);
            midPoint.transform.rotation = Avg[averageMode](left.transform.rotation, right.transform.rotation);
        }
    }
}
