using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KW_Mocap
{
    /// <summary>
    /// カメラの移動を扱うクラス。カメラコンポーネントと一緒に使う。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        #region StaticField
        /* 標準的な位置のプリセット */
        private static Vector3 pos1 = new Vector3(0.0f, -17.4f, -11.01941f);
        private static Quaternion rot1 = Quaternion.Euler(60.0f, 0.0f, 0.0f);
        private static Vector3 pos2 = new Vector3(0.0f, 18.1077747f, -2.13255429f);
        private static Quaternion rot2 = new Quaternion(0.676393151f, 8.08870536e-05f, -8.61902517e-05f, 0.736540854f);

        /* シーンを跨ぐときに最後の位置と回転を記憶するための変数 */
        private static Vector3 lastPosition = pos1;
        private static Quaternion lastRotation = rot1;

        /// <summary>
        /// カメラが動ける位置のy座標の上限
        /// </summary>
        private static readonly float ceiling = 30.0f;
        /// <summary>
        /// カメラが動ける位置のy座標の下限
        /// </summary>
        private static readonly float floor = -30.0f;
        #endregion

        #region Field
        private bool isActive = true;
        private Vector3 preMousePos;
        Button camera1, camera2;

        [SerializeField]
        private Vector3 rotCenter = Vector3.zero;

        [SerializeField, Range(0.0f, 100.0f)]
        private float moveSpead = 50.0f;
        private float MoveSpeed
        {
            get => moveSpead * 0.5f;
        }

        [SerializeField, Range(0.0f, 100.0f)]
        private float zoomSpeed = 50.0f;
        private float ZoomSpeed
        {
            get => zoomSpeed * 10.0f;
        }

        [SerializeField]
        private RotateSpeed rotateSpeed = new RotateSpeed(50.0f, 50.0f);

        [SerializeField]
        private LocalYAsisStabilization forceLocalYAxisUp = LocalYAsisStabilization.ForceLocalYAxisUp;
        #endregion

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
            SetLastSceneTransform();
            UISetting.SetButton(ref camera1, "Camera1", OnBtn_Camera1);
            UISetting.SetButton(ref camera2, "Camera2", OnBtn_Camera2);
        }

        #region CameraMotion
        void Update()
        {
            if (!isActive) return;

            MouseControl();
            KeyControl();

            /* 上向き補正 */
            switch (forceLocalYAxisUp)
            {
                case LocalYAsisStabilization.ForceLocalYAxisUp:
                    ForceLocalYAxisUp();
                    break;
                case LocalYAsisStabilization.transformLookAt:
                    transform.LookAt(rotCenter);
                    /* ↓でも同じ */
                    //transform.rotation = Quaternion.LookRotation(rotCenter - transform.position);
                    break;
                case LocalYAsisStabilization.None:
                    break;
            }
        }

        /// <summary>
        /// 水平方向のカメラの平行移動
        /// </summary>
        /// <param name="amout">正だと右。負だと左。</param>
        private void HorizontalMove(float amout)
        {
            transform.Translate(amout * MoveSpeed * Time.deltaTime * transform.right, Space.World);
        }

        /// <summary>
        /// 垂直方向のカメラの平行移動
        /// </summary>
        /// <param name="amout">正だと前進。負だと後退。</param>
        private void VerticalMove(float amout)
        {
            /* transform.forwardベクトルのxz平面への正射影を正規化したベクトル */
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            transform.Translate(amout * MoveSpeed * Time.deltaTime * forward, Space.World);
        }

        /// <summary>
        /// カメラのズームイン/アウト
        /// </summary>
        /// /// <param name="amount">正だと前進。負だと後退。</param>
        private void Zoom(float amount)
        {
            bool goingUp = !(transform.forward.y > 0 ^ amount > 0);
            bool canGoUp = goingUp && transform.position.y < ceiling;
            bool canGoDown = !goingUp && transform.position.y > floor;
            if (canGoUp || canGoDown)
                transform.Translate(amount * ZoomSpeed * Time.deltaTime * transform.forward, Space.World);
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
        /// カメラのディスプレイを中心とした水平方向の回転。
        /// 点rotCenterを通る方向ベクトルtranform.rightを軸とするyAngle度の回転。
        /// </summary>
        /// <param name="amount">正だと左回り。負だと右回り。</param>
        private void HorizontalRotateAround(float amount)
        {
            float yAngle = GetDistanceFromYAxis() * amount * rotateSpeed.Horizontal * Time.deltaTime;
            transform.RotateAround(rotCenter, transform.up, yAngle);
        }

        /// <summary>
        /// カメラのディスプレイを中心とした垂直方向の回転。
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
                float xAngle = amount * rotateSpeed.Vertical * Time.deltaTime;
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
        /// キー入力によるカメラ操作。
        /// </summary>
        private void KeyControl()
        {
            /* 移動 */
            if (Input.GetKey(KeyCode.D)) HorizontalMove(1.0f);
            if (Input.GetKey(KeyCode.A)) HorizontalMove(-1.0f);
            if (Input.GetKey(KeyCode.W)) VerticalMove(1.0f);
            if (Input.GetKey(KeyCode.S)) VerticalMove(-1.0f);
            if (Input.GetKey(KeyCode.E)) Zoom(1.0f);
            if (Input.GetKey(KeyCode.C)) Zoom(-1.0f);

            /* 回転 */
            if (Input.GetKey(KeyCode.LeftArrow)) HorizontalRotateAround(1.0f);
            if (Input.GetKey(KeyCode.RightArrow)) HorizontalRotateAround(-1.0f);
            if (Input.GetKey(KeyCode.UpArrow)) VerticalRotateAround(1.0f);
            if (Input.GetKey(KeyCode.DownArrow)) VerticalRotateAround(-1.0f);
        }

        /// <summary>
        /// マウスによるカメラ操作。
        /// </summary>
        private void MouseControl()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                preMousePos = Input.mousePosition;
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
                MouseDrag(Input.mousePosition);
            Zoom(Input.GetAxis("Mouse ScrollWheel") * 5.0f);
        }

        /// <summary>
        /// マウスのドラッグ操作。
        /// 右ボタンかホイールボタンが押されているときに呼ばれる。
        /// </summary>
        /// <param name="mousePos">現在のマウス位置</param>
        private void MouseDrag(Vector3 mousePos)
        {
            Vector3 diff = mousePos - preMousePos;
            if (diff.sqrMagnitude < Vector3.kEpsilon * Vector3.kEpsilon) return;

            /* 右クリックで回転 */
            if (Input.GetMouseButton(1))
            {
                HorizontalRotateAround(diff.x);
                VerticalRotateAround(-diff.y);
            }

            /* ホイールクリックで移動 */
            if (Input.GetMouseButton(2))
            {
                HorizontalMove(-diff.x / 2);
                VerticalMove(-diff.y / 2);
            }

            preMousePos = mousePos;
        }
        #endregion

        /// <summary>
        /// Camera1ボタンが押されたときに実行。カメラ位置をデフォルトに戻す。
        /// </summary>
        private void OnBtn_Camera1()
        {
            transform.SetPositionAndRotation(pos1, rot1);
        }

        /// <summary>
        /// Camera2ボタンが押されたときに実行。カメラ位置を俯瞰視点にする。
        /// </summary>
        private void OnBtn_Camera2()
        {
            transform.SetPositionAndRotation(pos2, rot2);
        }

        /// <summary>
        /// シーンをアンロードするときに現在のカメラの位置と回転を記憶する。
        /// </summary>
        public void HoldCurrentSceneTransform()
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        /// <summary>
        /// シーンをロードしたときに前のシーンのカメラの位置と回転をセットする。
        /// </summary>
        private void SetLastSceneTransform()
        {
            transform.SetPositionAndRotation(lastPosition, lastRotation);
        }
    }

    [Serializable]
    public struct RotateSpeed
    {
        [SerializeField, Range(0.0f, 100.0f)]
        private float horizontal;
        public float Horizontal
        {
            get => horizontal * 0.05f;
        }

        [SerializeField, Range(0.0f, 100.0f)]
        private float vertical;
        public float Vertical
        {
            get => vertical * 0.6f;
        }

        public RotateSpeed(float horizontal, float vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
    }
}
