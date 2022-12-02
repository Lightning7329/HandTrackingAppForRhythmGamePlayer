using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    public class LeapMotionCalibration : MonoBehaviour
    {
        [SerializeField] GameObject left, right, midPoint;
        [SerializeField] AverageMethod AverageMode = AverageMethod.Slerp;

        Button clbPosButton, clbRotButton;
        Text count;
        Vector3 midPos;
        Quaternion midRot;
        public Vector3 adjustPos = Vector3.zero;
        public Quaternion adjustRot = Quaternion.identity;

        public enum AverageMethod {Lerp, Slerp}
        delegate Quaternion AverageQuaternion(Quaternion q1, Quaternion q2);
        Dictionary<AverageMethod, AverageQuaternion> Avg = new Dictionary<AverageMethod, AverageQuaternion>() {
                { AverageMethod.Lerp,  (q1, q2) => Quaternion.Lerp(q1, q2, 0.5f) },
                { AverageMethod.Slerp, (q1, q2) => Quaternion.Slerp(q1, q2, 0.5f) }
            };


        void Start()
        {
            //UISetting.SetButton(ref clbPosButton, "CalibrationPosition", CalibratePosition);
            UISetting.SetButton(ref clbRotButton, "CalibrationRotation", () => StartCoroutine(CountDown(3)));
            count = GameObject.Find("Count").GetComponent<Text>();
            count.gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateMidPoint();
        }

        /// <summary>
        /// カウントダウンを行い、0秒になったらキャリブレーションが実行される。
        /// </summary>
        /// <param name="second">カウントダウンの秒数</param>
        /// <returns></returns>
        IEnumerator CountDown(int second)
        {
            count.gameObject.SetActive(true);
            count.text = second.ToString();
            var wait = new WaitForSeconds(1.0f);
            for (int i = second - 1; i >= 0; i--)
            {
                yield return wait;
                count.text = i.ToString();
                Debug.Log("countDown: " + i);
            }
            //CalibratePosition();
            CalibrateRotation();
            yield return new WaitForSeconds(0.5f);
            count.gameObject.SetActive(false);
        } 

        /// <summary>
        /// 位置のキャリブレーション。なお、LeapMotionの位置は原点以外置くとスケーリングおかしくなる。
        /// </summary>
        void CalibratePosition()
        {
            midPos = 0.5f * (left.transform.position + right.transform.position);
            Vector3 complement = adjustPos - midPos;
            this.transform.position = complement;
        }

        /// <summary>
        /// 回転のキャリブレーション。
        /// </summary>
        void CalibrateRotation()
        {
            midRot = Avg[AverageMode](left.transform.rotation, right.transform.rotation);
            this.transform.rotation = Quaternion.Inverse(midRot) * this.transform.rotation;
        }

        void UpdateMidPoint()
        {
            if (midPoint == null) return;
            midPoint.transform.position = 0.5f * (left.transform.position + right.transform.position);
            midPoint.transform.rotation = Avg[AverageMode](left.transform.rotation, right.transform.rotation);
        }
    }
}
