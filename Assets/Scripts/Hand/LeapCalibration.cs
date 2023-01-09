using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class LeapCalibration : MonoBehaviour
    {
        [SerializeField] GameObject left, right, hands;

        [SerializeField] AverageMethod averageMode = AverageMethod.Slerp;

        public Vector3 adjustPos = Vector3.zero;

        Vector3 midPos = new Vector3(0.0f, -33.883246f, -8.88f);
        Button calibrationButton;

        public enum AverageMethod {Lerp, Slerp}
        delegate Quaternion AverageQuaternion(Quaternion q1, Quaternion q2);

        readonly Dictionary<AverageMethod, AverageQuaternion> Avg
            = new Dictionary<AverageMethod, AverageQuaternion>() {
                { AverageMethod.Lerp,  (q1, q2) => Quaternion.Lerp(q1, q2, 0.5f) },
                { AverageMethod.Slerp, (q1, q2) => Quaternion.Slerp(q1, q2, 0.5f) }
            };


        void Start()
        {
            
        }

        void Update()
        {
            hands.transform.localPosition = -midPos + adjustPos;
        }

        /// <summary>
        /// カウントダウンを行い、0秒になったらキャリブレーションが実行される。
        /// </summary>
        /// <param name="second">カウントダウンの秒数</param>
        /// <returns></returns>
        public IEnumerator Calibration(Text count, int second)
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
    }
}
