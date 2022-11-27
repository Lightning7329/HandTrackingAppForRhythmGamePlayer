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

        [SerializeField]
        private Vector3 rotCenter = Vector3.zero;

        [SerializeField, Range(0.1f, 30.0f)]
        private float moveSpead = 10f;

        [SerializeField, Range(0.01f, 5.0f)]
        private float horizontalRotateSpead = 2.5f;

        [SerializeField, Range(0.1f, 70.0f)]
        private float verticalRotateSpead = 30f;

        [SerializeField, Range(0.1f, 30.0f)]
        private float zoomSpead = 10f;

        [SerializeField]
        private LocalYAsisStabilization forceLocalYAxisUp = LocalYAsisStabilization.ForceLocalYAxisUp;

        /// <summary>
        /// 上向き補正の手法
        /// </summary>
        public enum LocalYAsisStabilization
        {
            None,
            ForceLocalYAxisUp,
            transformLookAt
        }

        /// <summary>
        /// ファイル名入力画面などでは有効になってほしくないので、他のクラスに無効にしてもらうためのメソッド。
        /// </summary>
        /// <param name="flg">有効か無効か</param>
        public void SetActive(bool flg) => isActive = flg;

        void Start()
        {
            UISetting.SetButton(ref camera1, "Camera1", OnBtn_Camera1);
            UISetting.SetButton(ref camera2, "Camera2", OnBtn_Camera2);
        }

        void Update()
        {
            if (!isActive) return;

            Move();
            if (Input.GetKey(KeyCode.E)) Zoom(1.0f);
            if (Input.GetKey(KeyCode.C)) Zoom(-1.0f);
            if (Input.GetKey(KeyCode.LeftArrow)) HorizontalRotateAround(1.0f);
            if (Input.GetKey(KeyCode.RightArrow)) HorizontalRotateAround(-1.0f);
            if (Input.GetKey(KeyCode.UpArrow)) VerticalRotateAround(1.0f);
            if (Input.GetKey(KeyCode.DownArrow)) VerticalRotateAround(-1.0f);
            switch (forceLocalYAxisUp)
            {
                case LocalYAsisStabilization.ForceLocalYAxisUp:
                    ForceLocalYAxisUp();
                    break;
                case LocalYAsisStabilization.transformLookAt:
                    transform.LookAt(rotCenter);
                    //↓でも同じ
                    //transform.rotation = Quaternion.LookRotation(rotCenter - transform.position);
                    break;
                case LocalYAsisStabilization.None:
                    break;
            }
        }

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
        /// /// <param name="amount">正だと近く。負だと引く。</param>
        private void Zoom(float amount)
        {
            //TODO: ディプレイにめり込まないようにする
            transform.Translate(amount * zoomSpead * Time.deltaTime * transform.forward, Space.World);
        }

        /// <summary>
        /// 回転中心を通る上向きの軸からカメラまでの距離
        /// </summary>
        /// <returns></returns>
        private float GetDistanceFromYAxis()
        {
            var projectionVector = Vector3.Project(transform.position - rotCenter, Vector3.up);
            return Vector3.Distance(rotCenter + projectionVector, transform.position);
        }

        /// <summary>
        /// カメラのディスプレイを中心とした横方向の回転。
        /// 点rotCenterを通る方向ベクトルtranform.rightを軸とするyAngle度の回転。
        /// </summary>
        /// <param name="amount">正だと左回り。負だと右回り。</param>
        private void HorizontalRotateAround(float amount)
        {
            float yAngle = GetDistanceFromYAxis() * amount * horizontalRotateSpead * Time.deltaTime;
            transform.RotateAround(rotCenter, transform.up, yAngle);
        }

        /// <summary>
        /// カメラのディスプレイを中心とした縦方向の回転。
        /// 点rotCenterを通る方向ベクトルtranform.rightを軸とするyAngle度の回転。
        /// </summary>
        /// <param name="amount">正だと上回り。負だと下回り。</param>
        private void VerticalRotateAround(float amount)
        {
            /* カメラが上から回り込まないように俯角を制限 */
            float depression = Vector3.Angle(transform.up, Vector3.up);
            bool canGoDown = amount < 0 && depression > 5.0f;   // 下に回り込みたい && まだそんなに横向きじゃない -> まだ下行ける
            bool canGoUp = amount > 0 && depression < 85.0f;    // 上に回り込みたい && まだそんなに下向きじゃない -> まだ上行ける
            if (canGoDown || canGoUp)
            {
                float xAngle = amount * verticalRotateSpead * Time.deltaTime;
                transform.RotateAround(rotCenter, transform.right, xAngle);
            }
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
