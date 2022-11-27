using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KW_Mocap
{
    /// <summary>
    /// カメラの移動を扱うクラス。カメラコンポーネントと一緒に使う。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private bool isActive = true;
        Button camera1, camera2;
        [SerializeField] private Vector3 rotCenter = Vector3.zero;
        [SerializeField] private float moveSpead = 10f;
        [SerializeField] private float rotateSpead = 30f;
        [SerializeField] private float zoomSpead = 10f;
        [SerializeField] private bool forceLocalYAxisUp = true;

        void Start()
        {
            UISetting.SetButton(ref camera1, "Camera1", OnBtn_Camera1);
            UISetting.SetButton(ref camera2, "Camera2", OnBtn_Camera2);
        }

        void Update()
        {
            if (!isActive) return;

            Move();
            Zoom();
            RotateAround();
            if (forceLocalYAxisUp) ForceLocalYAxisUp();
        }

        /// <summary>
        /// ファイル名入力画面などでは有効になってほしくないので、他のクラスに無効にしてもらうためのメソッド。
        /// </summary>
        /// <param name="flg">有効か無効か</param>
        public void SetActive(bool flg) => isActive = flg;

        /// <summary>
        /// カメラの平行移動
        /// </summary>
        private void Move()
        {
            // カメラの向いてる方向のワールドxy平面に沿った方向ベクトル。言い換えるとローカルzの正射影ベクトルを正規化したもの。
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (Input.GetKey(KeyCode.W)) transform.Translate(moveSpead * Time.deltaTime * forward, Space.World);
            if (Input.GetKey(KeyCode.S)) transform.Translate(moveSpead * Time.deltaTime * -forward, Space.World);

            // カメラの向いてる方向の右向き
            var right = transform.right;
            if (Input.GetKey(KeyCode.D)) transform.Translate(moveSpead * Time.deltaTime * right, Space.World);
            if (Input.GetKey(KeyCode.A)) transform.Translate(moveSpead * Time.deltaTime * -right, Space.World);
        }

        /// <summary>
        /// カメラのズームイン/アウト
        /// </summary>
        private void Zoom()
        {
            if (Input.GetKey(KeyCode.E)) transform.Translate(zoomSpead * Time.deltaTime * transform.forward, Space.World);
            if (Input.GetKey(KeyCode.C)) transform.Translate(zoomSpead * Time.deltaTime * -transform.forward, Space.World);
        }

        /// <summary>
        /// カメラのディスプレイを中心とした回転
        /// </summary>
        private void RotateAround()
        {
            /* 
             * 横方向の回転。
             * 点rotCenterを通る方向ベクトルtranform.upを軸とするyAngle度の回転。
             */
            float yAngle = 0.0f;
            if (Input.GetKey(KeyCode.LeftArrow)) yAngle = rotateSpead * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow)) yAngle = -rotateSpead * Time.deltaTime;
            transform.RotateAround(rotCenter, transform.up, yAngle);

            /* 
             * 縦方向の回転。
             * 点rotCenterを通る方向ベクトルtranform.rightを軸とするyAngle度の回転。
             */
            float xAngle = 0.0f;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                /* ディスプレイの裏側にカメラが回り込まないように俯角を制限 */
                if (Vector3.Angle(transform.up, Vector3.up) > 5.0f)
                {
                    xAngle = rotateSpead * Time.deltaTime;
                }
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                /* カメラが上から回り込まないように俯角を制限 */
                if (Vector3.Angle(transform.up, Vector3.up) < 85.0f)
                {
                    xAngle = -rotateSpead * Time.deltaTime;
                }
            }
            transform.RotateAround(rotCenter, transform.right, -xAngle);
        }

        /// <summary>
        /// カメラのローカル上方向がなるべくワールド上方向を向くようにカメラを回転する。
        /// </summary>
        private void ForceLocalYAxisUp()
        {
            Vector3 projectionVector = Vector3.ProjectOnPlane(vector: Vector3.up, planeNormal: transform.forward);
            float angle = Vector3.SignedAngle(from: transform.up, to: projectionVector, axis: transform.forward);
            transform.Rotate(transform.forward, angle, Space.World);
        }

        /// <summary>
        /// Camera1ボタンが押されたときに実行。カメラ位置をデフォルトに戻す。
        /// </summary>
        private void OnBtn_Camera1()
        {
            var pos = new Vector3(0.0f, 19.95238f, -11.01941f);
            var rot = Quaternion.Euler(new Vector3(60.27f, 0.0f, 0.0f));
            transform.SetPositionAndRotation(pos, rot);
        }

        /// <summary>
        /// Camera2ボタンが押されたときに実行。カメラ位置を俯瞰視点にする。
        /// </summary>
        private void OnBtn_Camera2()
        {
            var pos = new Vector3(0.0f, 18.2f, 0.0f);
            var rot = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
